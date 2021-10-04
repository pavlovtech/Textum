using System.Threading.Tasks;

namespace TextumReader.TranslationsCollectorWorkerService.Abstract
{
    public interface ITranslationsProcessingService
    {
        Task Run();
    }
}