using ABCRetail.Models;
using ABCRetail.Services.AzureFunctions;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly AzureFunctionsService _azureFunctionsService;

        public OrdersController(
            QueueStorageService queueStorageService,
            AzureFunctionsService azureFunctionsService)
        {
            _queueStorageService = queueStorageService;
            _azureFunctionsService = azureFunctionsService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            OrderText order)
        {
            if (!ModelState.IsValid)
            {
                return View(order);
            }

            order.TransactionType = "Order";
            order.Timestamp = DateTime.UtcNow;

            // 1. Send transaction to Azure Queue
            await _queueStorageService
                .SendMessageAsync(order);

            // 2. Create transaction log
            var logContent = $"""
    ABC RETAIL TRANSACTION LOG
    ===========================

    Transaction Type: Order Processing
    Order ID: {order.OrderId}
    Customer: {order.CustomerName}
    Product: {order.ProductName}
    Quantity: {order.Quantity}
    Transaction Date: {order.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
    Status: Order Added to Processing Queue
    """;

            await _azureFunctionsService.PostTextAsync(
                "WriteTransactionLogFunction",
                logContent);

            TempData["SuccessMessage"] =
                "Order successfully processed.";

            return RedirectToAction(nameof(Create));
        }
    }
}
