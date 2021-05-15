using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;


namespace TextumReader.Services.Translator.DTO.Responses
{
    public record WordTranslation(string Translation, string PartOfSpeech);

    [ResponseCache(Duration = 7 * 24 * 60 * 60, Location = ResponseCacheLocation.Any)]
    public record WordTranslations(string Word, IEnumerable<WordTranslation> Translations);

    public record TextTranslation(string Translation);

    public record TranslationRequest(string From, string To, string Text);
}
