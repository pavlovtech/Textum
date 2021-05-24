using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextumReader.GoogleTranslateScrapper
{
    public class WordTranslation
    {
        public string Translation { get; set; }
        public string PartOfSpeach { get; set; }
        public string Frequency { get; set; }
        public IEnumerable<string> Synonyms { get; set; }
        public IEnumerable<string> Examples { get; set; }
    }

    public class TranslationEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Word { get; init; }
        public string MainTranslation { get; init; }
        public IEnumerable<WordTranslation> Translations { get; init; }
    }
}
