using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TextumReader.Services.Dictionary.Models
{
    public class Word
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DictionaryId { get; set; }

        public string DisplayWord { get; set; }

        public DateTimeOffset WordCreated { get; set; }

        public IEnumerable<WordTranslation> Translations { get; set; }
    }
}