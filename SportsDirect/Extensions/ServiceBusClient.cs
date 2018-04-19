using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace SportsDirect
{
    public class ServiceBusClientCredentials
    {
        public static string ConnectionString { get; set; }
        public static string Key { get; set; }

        public static string QueueName { get; set; }
    }

    public static class ServiceBusClient
    {
        private static IQueueClient queueClient;

        public static void Initialize()
        {
            queueClient = new QueueClient(ServiceBusClientCredentials.ConnectionString, ServiceBusClientCredentials.QueueName);
        }

        //Send message to Service Bus queue
        public static async Task SendMessageAsync(string message)
        {
            try
            {
                var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
                //Add a session ID which ensures first in first out (NOTE: receiver has to be configured to ingest this correctly)
                encodedMessage.SessionId = Guid.NewGuid().ToString();

                // Send the message to the queue.
                await queueClient.SendAsync(encodedMessage);
                await queueClient.CloseAsync();
            }
            catch (Exception exception)
            {
                string msg = $"{DateTime.Now} :: Exception: {exception.Message}";
                throw new ApplicationException(msg);
            }
        }
    }   
}

