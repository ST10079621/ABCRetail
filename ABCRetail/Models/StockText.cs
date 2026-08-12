namespace ABCRetail.Models
{
    public class StockText
    {
        public string TransactionType { get; set; } = "Stock";
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
