using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TextumReader.Services.TextMaterial.Models;

namespace TextumReader.Services.TextMaterial.Services
{
    public class TextsService
    {
        private readonly DatabaseSettings _options;
        private readonly IMongoCollection<Text> _texts;

        public TextsService(IOptions<DatabaseSettings> settings)
        {
            _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _texts = database.GetCollection<Text>(_options.CollectionName);
        }

        public List<Text> Get()
        {
            return _texts.Find(book => true).ToList();
        }

        public Text Get(string id)
        {
            return _texts.Find(book => book.Id == id).FirstOrDefault();
        }

        public Text Create(Text text)
        {
            _texts.InsertOne(text);
            return text;
        }

        public void Update(string id, Text text)
        {
            _texts.ReplaceOne(book => book.Id == id, text);
        }

        public void Remove(Text text)
        {
            _texts.DeleteOne(book => book.Id == text.Id);
        }

        public void Remove(string id)
        {
            _texts.DeleteOne(book => book.Id == id);
        }
    }
}