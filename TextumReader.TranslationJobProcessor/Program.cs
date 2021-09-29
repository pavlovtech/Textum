using System.Linq;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TextumReader.TranslationJobProcessor.Jobs;
using TextumReader.TranslationJobProcessor.Models;
using TextumReader.TranslationJobProcessor.Services;

namespace TextumReader.TranslationJobProcessor
{
    class Program
    {
        private static string connectionString =
            "Endpoint=sb://textum-service-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=plILlKunrYGfN0jCGBpCh9W3Fo2EgSz9NGMUmoPxlIQ=";
        static string queueName = "words-queue";

        static async Task Main()
        {
            using var log = new LoggerConfiguration()
            .WriteTo
            .Console()
            .CreateLogger();

            var examplesService = new CognitiveServicesTranslator();

            var proxyProvider = new ProxyProvider();

            await using var client = new ServiceBusClient(connectionString);

            var receiver = client.CreateReceiver(queueName);

            var cosmosClient = new CosmosClient("AccountEndpoint=https://textum-db.documents.azure.com:443/;AccountKey=wW2rIFDePw7LUkS1vgrAgtSsqH5DgOK36aDncGa2tlmZCMH8fPGKtENk6XuSr6DJXhkkFc96QGsx9H8tFKrhEw==;",
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                });

            var msgs = await receiver.ReceiveMessagesAsync(30);

            while (msgs.Count > 0)
            {
                var tasks = msgs.Select(m =>
                {
                    var (@from, to, words) = JsonConvert.DeserializeObject<TranslationRequest>(m.Body.ToString());

                    var job = new GetTranslationsJob(cosmosClient, receiver, examplesService, proxyProvider, log);

                    var task = new Task(async () => await job.Run(@from, to, words, m));

                    task.Start();

                    return task;
                });

                await Task.WhenAll(tasks);

                msgs = await receiver.ReceiveMessagesAsync(30);
            }
        }
    }
}
