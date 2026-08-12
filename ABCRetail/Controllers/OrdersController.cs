using ABCRetail.Services.AzureStorage;
using ABCRetail.Models;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly FileStorageService _fileStorageService;

        public OrdersController(
            QueueStorageService queueStorageService,
            FileStorageService fileStorageService)
        {
            _queueStorageService = queueStorageService;
            _fileStorageService = fileStorageService;
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
            var logFileName =
                $"{order.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}.log";

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

            await _fileStorageService.CreateLogAsync(
                logFileName,
                logContent);

            TempData["SuccessMessage"] =
                "Order successfully processed.";

            return RedirectToAction(nameof(Create));
        }
    }
}
