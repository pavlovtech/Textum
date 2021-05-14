using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TextumReader.Services.Translator.DTO.Responses
{
    [ResponseCache(Duration = 7 * 24 * 60 * 60, Location = ResponseCacheLocation.Any)]
    public class WordTranslationsDto
    {
        public string Word { get; set; }
        public IEnumerable<WordTranslationDto> Translations { get; set; }
    }
}