namespace ABCRetail.Functions.Models
{
    public class ProductRequest
    {
        public string PartitionKey { get; set; } = "Products";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}