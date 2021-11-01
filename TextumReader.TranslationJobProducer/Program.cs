using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;
using MoreLinq;
using Newtonsoft.Json;

namespace TextumReader.TranslationJobProducer
{
    record TranslationRequest(string From, string To, IEnumerable<string> Words);

    class Program
    {
        static string connectionString = "Endpoint=sb://textum.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Ov8isE9gIz9yd/uM0MSW3Dw4X3fVTGFNbBntx2wlqzw=";
        static string queueName = "words-queue";

        static async Task Main(string[] args)
        {
            string from = args[0];
            string to = args[1];
            string filePath = args[2];

            int batchSize = 30;
            var source = (await File.ReadAllLinesAsync(filePath)).Batch(batchSize);

            // create a Service Bus client 
            await using var client = new ServiceBusClient(connectionString);
            // create a sender for the queue 
            ServiceBusSender sender = client.CreateSender(queueName);

            foreach (var words in source)
            {
                var req = new TranslationRequest(from, to, words);

                // create a message that we can send
                var jsonReq = JsonConvert.SerializeObject(req);
                var message = new ServiceBusMessage(jsonReq);

                // send the message
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Sent message: {jsonReq}");
            }

            Console.WriteLine($"Done");
            Console.ReadKey();
        }
    }
}
