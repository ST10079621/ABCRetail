namespace ABCRetail.Models
{
    public class OrderText
    {
        public string TransactionType { get; set; } = "Order";
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
