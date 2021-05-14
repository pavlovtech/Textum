using System.Collections.Generic;
using System.Threading.Tasks;
using TextumReader.Services.Translator.DTO.Responses;

namespace TextumReader.Services.Translator.Services
{
    public interface ITranslator
    {
        Task<WordTranslationsDto> GetWordTranslation(string from, string to, string text);
        Task<IEnumerable<string>> GetExamples(string from, string to, string text, string translation);
        Task<TextTranslationDto> GetTextTranslation(string from, string to, string text);
    }
}
