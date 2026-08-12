using ABCRetail.Services.AzureStorage;
using ABCRetail.Models;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class StockController : Controller
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly FileStorageService _fileStorageService;

        public StockController(
            QueueStorageService queueStorageService,
            FileStorageService fileStorageService)
        {
            _queueStorageService = queueStorageService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            StockText stock)
        {
            if (!ModelState.IsValid)
            {
                return View(stock);
            }

            stock.TransactionType = "Stock";
            stock.Timestamp = DateTime.UtcNow;

            // 1. Send transaction to Azure Queue
            await _queueStorageService
                .SendMessageAsync(stock);

            // 2. Create transaction log
            var logFileName =
                $"STOCK_{DateTime.UtcNow:yyyyMMddHHmmssfff}.log";

            var logContent = $"""
                ABC RETAIL TRANSACTION LOG
                ===========================

                Transaction Type: Stock Management
                Product: {stock.ProductName}
                Quantity: {stock.Quantity}
                Action: {stock.Action}
                Transaction Date: {stock.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
                Status: Stock Update Added to Queue
                """;

            await _fileStorageService.CreateLogAsync(
                logFileName,
                logContent);

            TempData["SuccessMessage"] =
                "Stock transaction successfully processed.";

            return RedirectToAction(nameof(Update));
        }
    }
}
