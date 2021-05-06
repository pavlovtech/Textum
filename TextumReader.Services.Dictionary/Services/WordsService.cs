using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TextumReader.Services.Words.Models;
using TextumReader.Services.Words.Settings;

namespace TextumReader.Services.Dictionary.Services
{
    public class WordsService
    {
        private readonly DatabaseSettings _options;
        private readonly IMongoCollection<Word> _words;

        public WordsService(IOptions<DatabaseSettings> settings)
        {
            _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _words = database.GetCollection<Word>(_options.CollectionName);
        }

        public List<Word> GetWords()
        {
            return _words.Find(word => true).ToList();
        }

        public List<Word> GetWords(string userId)
        {
            return _words.Find(word => word.UserId == userId).ToList();
        }

        public Word GetWord(string wordId)
        {
            return _words.Find(word => word.Id == wordId).FirstOrDefault();
        }

        public Word Create(Word word)
        {
            _words.InsertOne(word);
            return word;
        }

        public void Update(string id, Word word)
        {
            _words.ReplaceOne(w => w.Id == id, word);
        }

        public void Remove(Word word)
        {
            _words.DeleteOne(w => w.Id == word.Id);
        }

        public void Remove(string id)
        {
            _words.DeleteOne(word => word.Id == id);
        }
    }
}