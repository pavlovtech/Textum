using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TextumReader.GoogleTranslateScrapper;
using TextumReader.TranslationJobProcessor.Abstract;
using TextumReader.TranslationJobProcessor.Models;

namespace TextumReader.TranslationJobProcessor
{
    public class TranslationsProcessingService : ITranslationsProcessingService
    {
        private readonly ITranslationEventHandler _translationEventHandler;
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<TranslationsProcessingService> _logger;
        private readonly IConfiguration _config;

        public TranslationsProcessingService(ITranslationEventHandler translationEventHandler, CosmosClient cosmosClient, ILogger<TranslationsProcessingService> logger, IConfiguration config)
        {
            _translationEventHandler = translationEventHandler;
            _cosmosClient = cosmosClient;
            _logger = logger;
            _config = config;
        }

        public async Task Run()
        {
            await using var client = new ServiceBusClient(_config.GetValue<string>("ServiceBusConnectionString"));

            var receiver = client.CreateReceiver(_config.GetValue<string>("QueueName"));

            var msgs = await receiver.ReceiveMessagesAsync(30);

            while (msgs.Count > 0)
            {
                var tasks = msgs.Select(m =>
                {
                    var task = new Task(async () =>
                    {
                        try
                        {
                            var translationEntities = await _translationEventHandler.Handle(m);

                            await SaveTranslations(translationEntities);

                            await receiver.CompleteMessageAsync(m);
                        }
                        catch (ProxyCompromizedException e)
                        {
                            await receiver.AbandonMessageAsync(m);
                            _logger.LogError(e, "IP is compromised");
                        }
                    });

                    task.Start();

                    return task;
                });

                await Task.WhenAll(tasks);

                msgs = await receiver.ReceiveMessagesAsync(30);
            }
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
                }
            }
        }
    }
}