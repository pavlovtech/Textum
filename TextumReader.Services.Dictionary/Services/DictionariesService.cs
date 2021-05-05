using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TextumReader.Services.Dictionary.Models;
using TextumReader.Services.Dictionary.Settings;

namespace TextumReader.Services.Dictionary.Services
{
    public class DictionariesService
    {
        private readonly DatabaseSettings _options;
        private readonly IMongoCollection<WordsDictionary> _dictionaries;

        public DictionariesService(IOptions<DatabaseSettings> settings)
        {
            _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _dictionaries = database.GetCollection<WordsDictionary>(_options.DictionariesCollectionName);
        }

        public List<WordsDictionary> Get()
        {
            return _dictionaries.Find(dict => true).ToList();
        }

        public WordsDictionary Get(string id)
        {
            return _dictionaries.Find(dict => dict.Id == id).FirstOrDefault();
        }

        public WordsDictionary Create(WordsDictionary dictionary)
        {
            _dictionaries.InsertOne(dictionary);
            return dictionary;
        }

        public void Update(string id, WordsDictionary dictionary)
        {
            _dictionaries.ReplaceOne(dict => dict.Id == id, dictionary);
        }

        public void Remove(WordsDictionary dictionary)
        {
            _dictionaries.DeleteOne(dict => dict.Id == dictionary.Id);
        }

        public void Remove(string id)
        {
            _dictionaries.DeleteOne(dict => dict.Id == id);
        }
    }
}