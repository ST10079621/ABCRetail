using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ABCRetail.Services.AzureFunctions
{
    public class AzureFunctionsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AzureFunctionsService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private string GetFunctionUrl(string functionName)
        {
            var baseUrl =
                _configuration["AzureFunctions:BaseUrl"];

            var functionKey =
                _configuration[
                    $"AzureFunctions:Keys:{functionName}"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "Azure Functions BaseUrl is missing.");
            }

            if (string.IsNullOrWhiteSpace(functionKey))
            {
                throw new InvalidOperationException(
                    $"Azure Function key is missing for '{functionName}'.");
            }

            var url =
                $"{baseUrl.TrimEnd('/')}/api/{functionName}";

            url +=
                $"?code={Uri.EscapeDataString(functionKey)}";

            return url;
        }

        public async Task<string> PostJsonAsync<T>(
            string functionName,
            T data)
        {
            var url = GetFunctionUrl(functionName);

            var json =
                JsonSerializer.Serialize(data);

            using var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            var response =
                await _httpClient.PostAsync(
                    url,
                    content);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Azure Function '{functionName}' failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> PostTextAsync(
    string functionName,
    string text)
        {
            var url = GetFunctionUrl(functionName);

            using var content =
                new StringContent(
                    text,
                    Encoding.UTF8,
                    "text/plain");

            var response =
                await _httpClient.PostAsync(
                    url,
                    content);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Azure Function '{functionName}' failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> UploadFileAsync(
            string functionName,
            Stream fileStream,
            string fileName,
            string contentType)
        {
            var url = GetFunctionUrl(functionName);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

            request.Headers.Add(
                "x-file-name",
                fileName);

            var content =
                new StreamContent(fileStream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    contentType);

            request.Content = content;

            var response =
                await _httpClient.SendAsync(request);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Azure Function '{functionName}' failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            return responseContent;
        }
    }
}