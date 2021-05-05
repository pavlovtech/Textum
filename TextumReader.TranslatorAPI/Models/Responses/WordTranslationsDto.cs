using System.Collections.Generic;

namespace TextumReader.Services.Translator.Models.Responses
{
    public class WordTranslationsDto
    {
        public string Word { get; set; }
        public IEnumerable<WordTranslationDto> Translations { get; set; }
    }
}