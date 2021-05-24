using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TextumReader.GoogleTranslateScrapper
{

    public class ConfigurationService
    {
        private readonly IMongoCollection<ScrapperConfig> _configCollection;

        public ConfigurationService(IOptions<DatabaseSettings> settings)
        {
            var _options = settings.Value;

            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);

            _configCollection = database.GetCollection<ScrapperConfig>(_options.configurationCollectionName);
        }


        public void Update(ScrapperConfig crapperConfig)
        {
            var result =_configCollection.ReplaceOne(x => x.ConfigName == crapperConfig.ConfigName, crapperConfig);
        }

        public ScrapperConfig Get(string configName)
        {
            return _configCollection.Find(x => x.ConfigName == configName).FirstOrDefault();
        }
    }
}
