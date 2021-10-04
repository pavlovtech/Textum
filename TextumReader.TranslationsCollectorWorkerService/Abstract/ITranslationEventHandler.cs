using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TextumReader.TranslationsCollectorWorkerService.Models;

namespace TextumReader.TranslationsCollectorWorkerService.Abstract
{
    public interface ITranslationEventHandler
    {
        Task<List<TranslationEntity>> Handle(ServiceBusReceivedMessage m);
    }
}