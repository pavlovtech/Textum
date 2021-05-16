using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TextumReader.Services.TextMaterial.Models;

namespace TextumReader.Services.TextMaterial.Services
{
    public class CosmosDbService : IRepository<Text>
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(Text item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<Text>(id, new PartitionKey(id));
        }

        public async Task<Text> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<Text> response = await _container.ReadItemAsync<Text>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Text>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<Text>(new QueryDefinition(queryString));
            var results = new List<Text>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, Text item)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(id));
        }
    }
}
