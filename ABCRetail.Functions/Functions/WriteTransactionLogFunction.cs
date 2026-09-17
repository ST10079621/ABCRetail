using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Functions.Functions
{
    public class WriteTransactionLogFunction
    {
        private readonly IConfiguration _configuration;

        public WriteTransactionLogFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("WriteTransactionLogFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var logContent = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(logContent))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Log content cannot be empty.");

                    return badResponse;
                }

                var connectionString =
                    _configuration["AzureStorage"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "AzureStorage connection string is missing.");
                }

                var shareName = "transaction-logs";

                var shareClient =
                    new ShareClient(
                        connectionString,
                        shareName);

                await shareClient.CreateIfNotExistsAsync();

                var directoryClient =
                    shareClient.GetRootDirectoryClient();

                var fileName =
                    $"Transaction_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.log";

                var fileClient =
                    directoryClient.GetFileClient(fileName);

                var bytes =
                    Encoding.UTF8.GetBytes(logContent);

                using var stream =
                    new MemoryStream(bytes);

                await fileClient.CreateAsync(bytes.Length);

                await fileClient.UploadAsync(stream);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message =
                        "Transaction log created successfully.",
                    share = shareName,
                    fileName = fileName
                });

                return response;
            }
            catch (Exception ex)
            {
                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    $"Error: {ex.Message}");

                return response;
            }
        }
    }
}