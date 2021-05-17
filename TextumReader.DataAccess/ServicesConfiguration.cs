using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace TextumReader.DataAccess
{
	public static class ServicesConfiguration
	{
		public static void AddCosmosDbService<T> (this IServiceCollection services, string databaseName, string containerName, string account, string key) where T: BaseModel
		{
			services.AddSingleton<IRepository<T>>(InitializeCosmosClientInstanceAsync<T>(databaseName, containerName, account, key).GetAwaiter().GetResult());
        }

        static async Task<CosmosDbService<T>> InitializeCosmosClientInstanceAsync<T>(string databaseName, string containerName, string account, string key) where T : BaseModel
        {
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key, new Microsoft.Azure.Cosmos.CosmosClientOptions()
            {
                SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
                {
                    PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
                }
            });
            var cosmosDbService = new CosmosDbService<T>(client, databaseName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }
    }
}
