using Azure.Storage.Files.Shares;

namespace ABCRetail.Services.AzureStorage
{
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _shareClient = new ShareClient(
                connectionString,
                "logs");

            _shareClient.CreateIfNotExists();
        }

        public async Task CreateLogAsync(
            string fileName,
            string logContent)
        {
            var directoryClient =
                _shareClient.GetRootDirectoryClient();

            var fileClient =
                directoryClient.GetFileClient(fileName);

            byte[] content =
                System.Text.Encoding.UTF8.GetBytes(logContent);

            using var stream =
                new MemoryStream(content);

            await fileClient.CreateAsync(stream.Length);

            await fileClient.UploadAsync(stream);
        }

        public async Task<List<string>> GetFileNamesAsync()
        {
            var directoryClient =
                _shareClient.GetRootDirectoryClient();

            var fileNames = new List<string>();

            await foreach (
                var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    fileNames.Add(item.Name);
                }
            }

            return fileNames;
        }

        public async Task<Stream> DownloadAsync(
            string fileName)
        {
            var directoryClient =
                _shareClient.GetRootDirectoryClient();

            var fileClient =
                directoryClient.GetFileClient(fileName);

            var response =
                await fileClient.DownloadAsync();

            var memoryStream = new MemoryStream();

            await response.Value.Content.CopyToAsync(
                memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}