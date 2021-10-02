using System.Threading.Tasks;

namespace TextumReader.TranslationJobProcessor.Abstract
{
    public interface ITranslationsProcessingService
    {
        Task Run();
    }
}