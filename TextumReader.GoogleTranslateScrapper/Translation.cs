using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextumReader.GoogleTranslateScrapper
{
    public class Translation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Word { get; init; }
        public IEnumerable<string> Translations { get; init; }
    }
}
