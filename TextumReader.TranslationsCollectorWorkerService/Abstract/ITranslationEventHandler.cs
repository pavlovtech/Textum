using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using TextumReader.TranslationsCollectorWorkerService.Models;

namespace TextumReader.TranslationsCollectorWorkerService.Abstract
{
    public interface ITranslationEventHandler
    {
        List<TranslationEntity> Handle(ServiceBusReceivedMessage message);
    }
}