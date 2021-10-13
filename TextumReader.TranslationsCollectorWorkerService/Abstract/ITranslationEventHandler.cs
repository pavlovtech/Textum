using System.Collections.Generic;
using System.Threading;
using Azure.Messaging.ServiceBus;
using TextumReader.TranslationsCollectorWorkerService.Models;

namespace TextumReader.TranslationsCollectorWorkerService.Abstract
{
    public interface ITranslationEventHandler
    {
        List<TranslationEntity> Handle(ServiceBusReceivedMessage message, CancellationToken stoppingToken);
    }
}