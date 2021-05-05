using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TextumReader.Services.Dictionary.Models
{
    public class WordsDictionary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DictionaryCreated { get; set; }
    }
}