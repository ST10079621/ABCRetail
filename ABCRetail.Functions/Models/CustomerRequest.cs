namespace ABCRetail.Functions.Models
{
    public class CustomerRequest
    {
        public string PartitionKey { get; set; } = "Customers";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }
}