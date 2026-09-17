using ABCRetail.Models;
using Azure.Data.Tables;

namespace ABCRetail.Services.AzureStorage
{
    public class TableStorageService
    {
        private readonly string _connectionString;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("AzureStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "AzureStorage was NOT loaded from appsettings.json.");
            }

            _connectionString = connectionString;
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

        public async Task<Product?> GetProductByIdAsync(string rowKey)
        {
            var table = GetTableClient("Products");

            try
            {
                return await table.GetEntityAsync<Product>(
                    "Products",
                    rowKey);
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            var table = GetTableClient("Products");

            await table.UpdateEntityAsync(
                product,
                product.ETag,
                TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string rowKey)
        {
            var table = GetTableClient("Products");

            await table.DeleteEntityAsync(
                "Products",
                rowKey);
        }

        // AUTHENTICATION

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            var table = GetTableClient("Customers");

            await foreach (var customer in table.QueryAsync<Customer>(
                c => c.Email == email))
            {
                return customer;
            }

            return null;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var table = GetTableClient("Customers");

            await table.UpdateEntityAsync(
                customer,
                customer.ETag,
                TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string rowKey)
        {
            var table = GetTableClient("Customers");

            await table.DeleteEntityAsync(
                "Customers",
                rowKey);
        }


    }
}
