using ABCRetail.Models;
using Azure.Data.Tables;

namespace ABCRetail.Services.AzureStorage
{
    public class TableStorageService
    {
        private readonly string _connectionString;

        public TableStorageService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
        }

        private TableClient GetTableClient(string tableName)
        {
            var serviceClient =
                new TableServiceClient(_connectionString);

            var tableClient =
                serviceClient.GetTableClient(tableName);

            tableClient.CreateIfNotExists();

            return tableClient;
        }

       
        // CUSTOMERS
        

        public async Task AddCustomerAsync(Customer customer)
        {
            var table = GetTableClient("Customers");

            await table.AddEntityAsync(customer);
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var table = GetTableClient("Customers");

            var customers = new List<Customer>();

            await foreach (var customer in table.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        
        // PRODUCTS
       

        public async Task AddProductAsync(Product product)
        {
            var table = GetTableClient("Products");

            await table.AddEntityAsync(product);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var table = GetTableClient("Products");

            var products = new List<Product>();

            await foreach (var product in table.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }
    }
}
