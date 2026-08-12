using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace ABCRetail.Services.AzureStorage
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _queueClient = new QueueClient(
                connectionString,
                "order-processing");

            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync<T>(T message)
        {
            var json =
                JsonSerializer.Serialize(message);

            await _queueClient.SendMessageAsync(json);
        }

        public async Task<List<string>> GetMessagesAsync()
        {
            var messages = new List<string>();

            QueueMessage[] queueMessages =
                await _queueClient.ReceiveMessagesAsync(
                    maxMessages: 32);

            foreach (var message in queueMessages)
            {
                messages.Add(message.MessageText);
            }

            return messages;
        }
    }
}
