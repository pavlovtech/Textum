using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

        public List<Text> GetByUserId(string userId)
        {
            return _texts.AsQueryable().Where(book => book.UserId == userId).ToList();
        }

        public Text GetByBookId(string id)
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

        public void Remove(string id)
        {
            _texts.DeleteOne(book => book.Id == id);
        }
    }
}