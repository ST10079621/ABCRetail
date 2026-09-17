using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Functions.Functions
{
    public class ProcessOrderQueueFunction
    {
        private readonly IConfiguration _configuration;

        public ProcessOrderQueueFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("ProcessOrderQueueFunction")]
        public async Task Run(
            [QueueTrigger("order-processing", Connection = "AzureStorage")]
            string message)
        {
            var connectionString =
                _configuration["AzureStorage"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "AzureStorage connection string is missing.");
            }

            Console.WriteLine(
                $"Received queue message: {message}");

            var processedQueueClient =
                new QueueClient(
                    connectionString,
                    "processed-orders");

            await processedQueueClient.CreateIfNotExistsAsync();

            var processedMessage = new
            {
                Status = "Processed",
                OriginalMessage = message,
                ProcessedAt = DateTime.UtcNow
            };

            var json =
                JsonSerializer.Serialize(processedMessage);

            await processedQueueClient.SendMessageAsync(json);

            Console.WriteLine(
                "Message written to processed-orders queue.");
        }
    }
}