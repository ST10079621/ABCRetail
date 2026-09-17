using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Functions.Functions
{
    public class UploadProductImageFunction
    {
        private readonly IConfiguration _configuration;

        public UploadProductImageFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("UploadProductImageFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            try
            {
                var connectionString =
                    _configuration["AzureStorage"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "AzureStorage connection string is missing.");
                }

                var containerName = "product-images";

                var blobServiceClient =
                    new BlobServiceClient(connectionString);

                var containerClient =
                    blobServiceClient.GetBlobContainerClient(
                        containerName);

                await containerClient.CreateIfNotExistsAsync();

                var fileName =
                    req.Headers.TryGetValues(
                        "x-file-name",
                        out var fileNameValues)
                        ? fileNameValues.FirstOrDefault()
                        : null;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName =
                        $"product-{Guid.NewGuid()}.jpg";
                }

                var blobClient =
                    containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(
                    req.Body,
                    overwrite: true);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message =
                        "Product image uploaded successfully.",
                    container = containerName,
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