using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureStorageQueue
{
    class Program
    {
        public static readonly string queueName = "misraqueue";

        public static readonly string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

        public static readonly QueueClient queueClient = new QueueClient(connectionString, queueName);
        static async Task Main(string[] args)
        {
            
            await CreateQueueClientAsync();
            await PeekMessagesFromQueue();
            await UpdateMessageInTheQueueAsync();
            await DequeueMessageFromQueue();
            await GetLengthOfQueue();
            await DeleteAQueue();
        }

        private static async Task UpdateMessageInTheQueueAsync()
        {
            if (await queueClient.ExistsAsync())
            {
                var response = await queueClient.ReceiveMessagesAsync(4);
                foreach (var item in response.Value)
                {
                    await queueClient.UpdateMessageAsync(item.MessageId, item.PopReceipt, "THIS IS NEW", TimeSpan.FromSeconds(20));
                }
            }
        }

        private static async Task DeleteAQueue()
        {
            if(await queueClient.ExistsAsync())
            {
                await queueClient.DeleteAsync();
            }
        }

        private static async Task GetLengthOfQueue()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("getting length of the queue");
            if (await queueClient.ExistsAsync())
            {
                QueueProperties queueProperties = await queueClient.GetPropertiesAsync();
                Console.WriteLine($"The total message count is {queueProperties.ApproximateMessagesCount}");
            }
        }

        private static async Task DequeueMessageFromQueue()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("dequeuing messages from the queue");
            if(await queueClient.ExistsAsync())
            {
                //QueueMessage[] queueMessages = await queueClient.ReceiveMessagesAsync(4);
                
                var response = await queueClient.ReceiveMessagesAsync(4, TimeSpan.FromMinutes(5));
                //this timespan will increase the visibility timeout 

                foreach (var item in response.Value)
                {
                    Console.WriteLine($"Message body: {item.Body} Message Id: {item.MessageId}");
                    await queueClient.DeleteMessageAsync(item.MessageId, item.PopReceipt);
                }
            }
        }

        private static async Task PeekMessagesFromQueue()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Peeking messages from the queue");

            if (await queueClient.ExistsAsync())
            {
                PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(4);
                foreach (var item in peekedMessages)
                {
                    Console.WriteLine($"Message body: {item.Body} Message Id: {item.MessageId}");
                   
                }
            }
        }

        private static  async Task CreateQueueClientAsync()
        {
            
            await queueClient.CreateIfNotExistsAsync();
            if(await queueClient.ExistsAsync())
            {
                Console.WriteLine($"queue {queueClient.Name} exists");

                //sending a message to the queue
                await queueClient.SendMessageAsync("this is my first message to the misraqueue");
                await queueClient.SendMessageAsync("this is my second message to the misraqueue");

            }
        }
    }
}
