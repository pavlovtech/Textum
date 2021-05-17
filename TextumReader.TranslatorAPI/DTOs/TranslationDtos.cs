using System.Collections.Generic;

namespace TextumReader.Services.Translator.DTO.Responses
{
    public record WordTranslation(string Translation, string PartOfSpeech);
    public record WordTranslations(string Word, IEnumerable<WordTranslation> Translations);
    public record TextTranslation(string Translation);
    public record TranslationRequest(string From, string To, string Text);
}
