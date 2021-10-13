using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TextumReader.TranslationsCollectorWorkerService.Abstract;
using TextumReader.TranslationsCollectorWorkerService.Exceptions;
using TextumReader.TranslationsCollectorWorkerService.Models;

namespace TextumReader.TranslationsCollectorWorkerService
{
    public class GoogleTranslateScraperWorker : BackgroundService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<GoogleTranslateScraperWorker> _logger;
        private readonly ServiceBusReceiver _receiver;
        private readonly TelemetryClient _telemetryClient;
        private readonly IConfiguration _config;
        private readonly ITranslationEventHandler _translationEventHandler;
        private int _maxMessages;
        private IEnumerable<Task> _currentTasks;

        public GoogleTranslateScraperWorker(
            CosmosClient cosmosClient,
            ServiceBusReceiver receiver,
            ILogger<GoogleTranslateScraperWorker> logger,
            ITranslationEventHandler translationEventHandler,
            TelemetryClient telemetryClient,
            IConfiguration config)
        {
            _logger = logger;
            _translationEventHandler = translationEventHandler;
            _telemetryClient = telemetryClient;
            _config = config;

            _cosmosClient = cosmosClient;
            _receiver = receiver;
            _maxMessages = config.GetValue<int>("MaxMessages");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("ExecuteAsync Started");

                var msgs = await _receiver.ReceiveMessagesAsync(_maxMessages, TimeSpan.FromSeconds(10), stoppingToken);

                //_telemetryClient.TrackEvent("Service Bus messages received");

                while (msgs.Count > 0 && !stoppingToken.IsCancellationRequested)
                {
                    _currentTasks = msgs.Select(message =>
                    {
                        var task = new Task(async () =>
                        {
                            using (_telemetryClient.StartOperation<RequestTelemetry>("Translations scraping"))
                            {
                                try
                                {
                                    var translationEntities = _translationEventHandler.Handle(message, stoppingToken);

                                    await _receiver.RenewMessageLockAsync(message, stoppingToken);

                                    await SaveTranslations(translationEntities);

                                    await _receiver.CompleteMessageAsync(message, stoppingToken);
                                    //_telemetryClient.TrackEvent("Message completed");
                                }
                                catch (CompromisedException e)
                                {
                                    await _receiver.AbandonMessageAsync(message, null, stoppingToken);
                                    //_telemetryClient.TrackException(e);
                                    _logger.LogError(e, "IP is compromised");
                                }
                                catch (ServiceBusException ex)
                                {
                                    _logger.LogError(ex, "Error occurred");
                                    _telemetryClient.TrackException(ex);
                                }
                                catch (Exception ex)
                                {
                                    await _receiver.AbandonMessageAsync(message, null, stoppingToken);
                                    _logger.LogError(ex, "Error occurred");
                                    _telemetryClient.TrackException(ex);
                                }
                            }
                        });

                        task.Start();

                        return task;
                    });

                    await Task.WhenAll(_currentTasks);

                    Console.Clear();

                    msgs = await _receiver.ReceiveMessagesAsync(_maxMessages, TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (Exception e)
            {
                _telemetryClient.TrackException(e);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service is stopping.");
            await base.StopAsync(stoppingToken);

            await Task.WhenAll(_currentTasks);
        }

        private async Task SaveTranslations(List<TranslationEntity> translationEntities)
        {
            var container = _cosmosClient.GetContainer("TextumDB", "translations");

            foreach (var translationEntity in translationEntities)
            {
                try
                {
                    await container.CreateItemAsync(translationEntity);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Error occurred");
                    _telemetryClient.TrackException(ex);
                }
            }
        }
    }
}