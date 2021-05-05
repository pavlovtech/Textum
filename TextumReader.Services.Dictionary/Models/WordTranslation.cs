using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TextumReader.Services.Dictionary.Models
{
    public class WordTranslation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string WordId { get; set; }
        public string Translation{ get; set; }

        public IEnumerable<string> Examples { get; set; }
    }
}