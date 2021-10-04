using System.Collections.Generic;

namespace TextumReader.TranslationsCollectorWorkerService.Models
{
    public record TranslationRequest(string From, string To, IList<string> Words);

    public class WordTranslation
    {
        public string Translation { get; set; }
        public string PartOfSpeech { get; set; }
        public string Frequency { get; set; }
        public IEnumerable<string> Synonyms { get; set; }
        public IEnumerable<string> Examples { get; set; }
    }

    public class TranslationEntity
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Id { get; set; }
        public string Word { get; init; }
        public string MainTranslation { get; init; }
        public IEnumerable<WordTranslation> Translations { get; init; }
    }
}