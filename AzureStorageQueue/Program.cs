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
                    //copied from msdn
                    //If the message represents a work task, you could use this feature to update the status of the work task. The following code updates the queue message with new contents, and sets the visibility timeout to extend another 60 seconds. This saves the state of work associated with the message, and gives the client another minute to continue working on the message. You could use this technique to track multistep workflows on queue messages, without having to start over from the beginning if a processing step fails due to hardware or software failure. Typically, you would keep a retry count as well, and if the message is retried more than n times, you would delete it.
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
                var response = await queueClient.ReceiveMessagesAsync(4, TimeSpan.FromSeconds(5));
                //this timespan will increase the visibility timeout, this is good for scenarios where you have a lot of work to do before deleting the message

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
                //Peek the count of messages as parameter.
                PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(4);
                foreach (var item in peekedMessages)
                {
                    Console.WriteLine($"Message body: {item.Body} Message Id: {item.MessageId}");
                   
                }
            }
        }

        private static  async Task CreateQueueClientAsync()
        { 
            //this will create the queue if it does not exists
            await queueClient.CreateIfNotExistsAsync();
            if(await queueClient.ExistsAsync())
            {
                Console.WriteLine($"queue {queueClient.Name} exists");
                //sending messages to the queue
                await queueClient.SendMessageAsync("this is my first message to the misraqueue");
                await queueClient.SendMessageAsync("this is my second message to the misraqueue");

            }
        }
    }
}
