using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TextumReader.GoogleTranslateScrapper
{
    public class ScrapperConfig
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ConfigName { get; set; }

        public int LastWord { get; set; }
    }
}
