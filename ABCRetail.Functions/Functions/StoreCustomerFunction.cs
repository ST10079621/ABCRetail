using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using ABCRetail.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Functions.Functions
{
    public class StoreCustomerFunction
    {
        private readonly IConfiguration _configuration;

        public StoreCustomerFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("StoreCustomerFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var json = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Request body is empty.");

                    return badResponse;
                }

                using var document =
                    JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty(
                        "type", out var typeProperty))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Please specify type as 'customer' or 'product'.");

                    return badResponse;
                }

                string type =
                    typeProperty.GetString()?.ToLower() ?? "";

                var connectionString =
                    _configuration["AzureStorage"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "AzureStorage connection string is missing.");
                }

                if (type == "customer")
                {
                    var customer =
                        JsonSerializer.Deserialize<CustomerRequest>(json);

                    if (customer == null)
                    {
                        throw new InvalidOperationException(
                            "Invalid customer data.");
                    }

                    var tableClient =
                        new TableClient(
                            connectionString,
                            "Customers");

                    await tableClient.CreateIfNotExistsAsync();

                    var entity = new TableEntity(
                        customer.PartitionKey,
                        customer.RowKey)
                    {
                        ["Name"] = customer.Name,
                        ["Email"] = customer.Email,
                        ["Phone"] = customer.Phone,
                        ["PasswordHash"] = customer.PasswordHash,
                        ["Role"] = customer.Role
                    };

                    await tableClient.AddEntityAsync(entity);

                    var response =
                        req.CreateResponse(HttpStatusCode.OK);

                    await response.WriteAsJsonAsync(new
                    {
                        message =
                            "Customer stored successfully.",
                        table = "Customers",
                        customer
                    });

                    return response;
                }

                if (type == "product")
                {
                    var product =
                        JsonSerializer.Deserialize<ProductRequest>(json);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            "Invalid product data.");
                    }

                    var tableClient =
                        new TableClient(
                            connectionString,
                            "Products");

                    await tableClient.CreateIfNotExistsAsync();

                    var entity = new TableEntity(
                         product.PartitionKey,
                         product.RowKey)
                    {
                        ["ProductName"] = product.ProductName,
                        ["Category"] = product.Category,
                        ["Price"] = product.Price,
                        ["Quantity"] = product.Quantity
                    };

                    await tableClient.AddEntityAsync(entity);

                    var response =
                        req.CreateResponse(HttpStatusCode.OK);

                    await response.WriteAsJsonAsync(new
                    {
                        message =
                            "Product stored successfully.",
                        table = "Products",
                        product
                    });

                    return response;
                }

                var invalidTypeResponse =
                    req.CreateResponse(
                        HttpStatusCode.BadRequest);

                await invalidTypeResponse.WriteStringAsync(
                    "Type must be 'customer' or 'product'.");

                return invalidTypeResponse;
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