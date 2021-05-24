using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TextumReader.GoogleTranslateScrapper
{
    public class TranslationService
    {
        private readonly IMongoCollection<TranslationEntity> _translations;

        public TranslationService(IOptions<DatabaseSettings> settings)
        {
            var _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _translations = database.GetCollection<TranslationEntity>(_options.EnRuCollectionName);

            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<TranslationEntity>("Word");
            var indexDefinition = new IndexKeysDefinitionBuilder<TranslationEntity>().Ascending(field);

            _translations.Indexes.CreateOne(new CreateIndexModel<TranslationEntity>(indexDefinition, options));
        }


        public TranslationEntity Insert(TranslationEntity translation)
        {
            if (_translations.Find(w => w.Word == translation.Word).Any())
            {
                return translation;
            }

            _translations.InsertOne(translation);
            return translation;
        }
    }
}
