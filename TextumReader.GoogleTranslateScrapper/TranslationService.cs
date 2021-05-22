using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TextumReader.GoogleTranslateScrapper
{
    public class TranslationService
    {
        private readonly IMongoCollection<Translation> _translations;

        public TranslationService(IOptions<DatabaseSettings> settings)
        {
            var _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _translations = database.GetCollection<Translation>(_options.CollectionName);
        }


        public Translation Create(Translation translation)
        {
            _translations.InsertOneAsync(translation);
            return translation;
        }
    }
}
