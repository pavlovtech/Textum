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
            var source = (await File.ReadAllLinesAsync(filePath)).Batch(batchSize).ToList();

            // create a Service Bus client 
            await using var client = new ServiceBusClient(connectionString);
            // create a sender for the queue 
            ServiceBusSender sender = client.CreateSender(queueName);

            var messages = source.Select(words =>
            {
                var req = new TranslationRequest(from, to, words);

                // create a message that we can send
                var jsonReq = JsonConvert.SerializeObject(req);
                return new ServiceBusMessage(jsonReq);
            }).Batch(400);

            foreach (var msgPortion in messages)
            {
                using var messageBatch = await sender.CreateMessageBatchAsync();

                foreach (var serviceBusMessage in msgPortion)
                {
                    // try adding a message to the batch
                    if (!messageBatch.TryAddMessage(serviceBusMessage))
                    {
                        // if it is too large for the batch
                        throw new Exception($"The message is too large to fit in the batch.");
                    }
                }

                // send the message
                await sender.SendMessagesAsync(messageBatch);
            }

            Console.WriteLine($"Done");
            Console.ReadKey();
        }
    }
}
