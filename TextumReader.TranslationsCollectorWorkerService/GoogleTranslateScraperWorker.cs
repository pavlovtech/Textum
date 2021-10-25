using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Konsole;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SerilogTimings;
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
        private List<Task> _currentTasks;

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(_config.GetValue<int>("MaxMessages").ToString());

            _maxMessages = _config.GetValue<int>("MaxMessages");

            try
            {
                _logger.LogInformation("ExecuteAsync Started");

                var msgs = await CollectMessages(stoppingToken);

                //_telemetryClient.TrackEvent("Service Bus messages received");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _currentTasks = msgs.Select(message => ProcessMessage(message, stoppingToken)).ToList();

                    await Task.WhenAll(_currentTasks);

                    _logger.LogInformation("Finished waiting for {count} tasks", _currentTasks.Count);

                    /*var chromeDriverProcesses = Process.GetProcessesByName("chromedriver");

                    Parallel.ForEach(chromeDriverProcesses, process =>
                    {
                        _logger.LogError("Killing chromedriver process {id}", process.Id);
                        process.Kill(false);
                    });

                    var chromeProcesses = Process.GetProcessesByName("chrome");

                    Parallel.ForEach(chromeProcesses, process =>
                    {
                        _logger.LogError("Killing chrome process {id}", process.Id);
                        process.Kill(false);
                    });*/

                    msgs = await CollectMessages(stoppingToken);

                    Console.Clear();
                }
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Service bus error in one of tasks");
                _telemetryClient.TrackException(ex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error occurred in one of tasks");
                _telemetryClient.TrackException(e);
                Console.WriteLine("Program crashed");
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private async Task<List<ServiceBusReceivedMessage>> CollectMessages(CancellationToken stoppingToken)
        {
            using var time = Operation.Time("GoogleTranslateScraperWorker.CollectMessages");

            var msgs = (await _receiver.ReceiveMessagesAsync(_maxMessages, TimeSpan.FromSeconds(3), stoppingToken)).ToList();

            _logger.LogInformation("Received messages: {count}", msgs.Count);

            for (int i = 0; msgs.Count < _maxMessages && i < 4; i++)
            {
                var messagesCount = _maxMessages - msgs.Count;

                _logger.LogInformation("Getting more messages: {count}", messagesCount);

                msgs.AddRange(await _receiver.ReceiveMessagesAsync(messagesCount, TimeSpan.FromSeconds(3),
                    stoppingToken));
            }

            _logger.LogInformation("Received messages: {count} total", msgs.Count);

            return msgs;
        }

        private async Task ProcessMessage(ServiceBusReceivedMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProcessMessage started for {id}", message.MessageId);

            using var op = _telemetryClient.StartOperation<RequestTelemetry>("Translations scraping");
            using var time = Operation.Time("GoogleTranslateScraperWorker.ProcessMessage");

            try
            {
                if (DateTimeOffset.UtcNow > message.LockedUntil)
                {
                    _logger.LogInformation("Lock expired on {date} for message {id}", message.LockedUntil.ToLocalTime(), message.MessageId);
                    _telemetryClient.TrackEvent("Lock expired");
                }

                await _receiver.RenewMessageLockAsync(message, stoppingToken);

                var translationEntities = await _translationEventHandler.Handle(message, stoppingToken);

                await SaveTranslations(translationEntities);

                _logger.LogInformation("SaveTranslations complete for message {id}", message.MessageId);

                await _receiver.CompleteMessageAsync(message, stoppingToken);

                _logger.LogInformation("ProcessMessage complete for {id}", message.MessageId);

                op.Telemetry.Success = true;
            }
            catch (CompromisedException e)
            {
                await _receiver.AbandonMessageAsync(message);
                _telemetryClient.TrackException(e);
                _logger.LogError(e, "IP is compromised");
                op.Telemetry.Success = false;
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Error occurred with service bus");
                _telemetryClient.TrackException(ex);
                op.Telemetry.Success = false;
            }
            catch (Exception ex)
            {
                await _receiver.AbandonMessageAsync(message);
                _logger.LogError(ex, "Unknown error occurred");
                _telemetryClient.TrackException(ex);
                op.Telemetry.Success = false;
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
            using var time = Operation.Time("GoogleTranslateScraperWorker.SaveTranslations");

            using (_telemetryClient.StartOperation<DependencyTelemetry>("SaveTranslations"))
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
}