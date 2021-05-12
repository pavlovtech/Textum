using System.Collections.Generic;
using System.Threading.Tasks;
using TextumReader.Services.Translator.Models.Requests;
using TextumReader.Services.Translator.Models.Responses;

namespace TextumReader.Services.Translator.Services
{
    public interface ITranslator
    {
        Task<WordTranslationsDto> GetWordTranslation(TranslationRequest translationRequest);
        Task<IEnumerable<string>> GetExamples(WordExampleRequest wordExampleRequest);
        Task<TextTranslationDto> GetTextTranslation(TranslationRequest translationRequest);
    }
}
