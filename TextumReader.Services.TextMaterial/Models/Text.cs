using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TextumReader.Services.TextMaterial.Models
{
    public class Text
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string TextContent { get; set; }
        public string InputLanguage { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
