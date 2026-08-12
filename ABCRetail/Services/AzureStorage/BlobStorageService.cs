using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.Services.AzureStorage
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient("product-images");

            _containerClient.CreateIfNotExists();
        }

        public async Task UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            var blobClient =
                _containerClient.GetBlobClient(fileName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(
                fileStream,
                options);
        }

        public async Task<List<string>> GetBlobNamesAsync()
        {
            var blobNames = new List<string>();

            await foreach (
                BlobItem blobItem
                in _containerClient.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }

            return blobNames;
        }

        public async Task<(Stream Stream, string ContentType)>
            DownloadAsync(string fileName)
        {
            var blobClient =
                _containerClient.GetBlobClient(fileName);

            var response =
                await blobClient.DownloadStreamingAsync();

            return (
                response.Value.Content,
                response.Value.Details.ContentType
            );
        }

        public async Task DeleteAsync(string fileName)
        {
            var blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
