using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
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
        private readonly ITranslationEventHandler _translationEventHandler;
        private const int MaxMessages = 30;

        public GoogleTranslateScraperWorker(
            CosmosClient cosmosClient,
            ServiceBusReceiver receiver,
            ILogger<GoogleTranslateScraperWorker> logger,
            ITranslationEventHandler translationEventHandler,
            TelemetryClient telemetryClient)
        {
            _logger = logger;
            _translationEventHandler = translationEventHandler;
            _telemetryClient = telemetryClient;

            _cosmosClient = cosmosClient;
            _receiver = receiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var msgs = await _receiver.ReceiveMessagesAsync(MaxMessages, null, stoppingToken);

            while (msgs.Count > 0 && !stoppingToken.IsCancellationRequested)
            {
                var tasks = msgs.Select(m =>
                {
                    var task = new Task(async () =>
                    {
                        using (_telemetryClient.StartOperation<RequestTelemetry>("GoogleTranslateScraperWorker.ExecuteAsync"))
                        {
                            _logger.LogInformation("ExecuteAsync Started");

                            try
                            {
                                var translationEntities = await _translationEventHandler.Handle(m);

                                await SaveTranslations(translationEntities);

                                await _receiver.CompleteMessageAsync(m, stoppingToken);
                            }
                            catch (CompromisedException e)
                            {
                                await _receiver.AbandonMessageAsync(m, null, stoppingToken);
                                _telemetryClient.TrackException(e);
                                _logger.LogError(e, "IP is compromised");
                            }
                            catch (Exception ex)
                            {
                                await _receiver.AbandonMessageAsync(m, null, stoppingToken);
                                _logger.LogError(ex, "Error occurred");
                                _telemetryClient.TrackException(ex);
                            }
                        }
                    });

                    task.Start();

                    return task;
                });

                await Task.WhenAll(tasks);

                msgs = await _receiver.ReceiveMessagesAsync(MaxMessages, null, stoppingToken);
            }
        }

        private async Task SaveTranslations(List<TranslationEntity> translationEntities)
        {
            var container = _cosmosClient.GetContainer("TextumDB", "translations");

            foreach (var translationEntity in translationEntities)
                try
                {
                    await container.CreateItemAsync(translationEntity);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Error occurred");
                }
        }
    }
}