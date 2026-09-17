using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly TableStorageService _tableStorage;

        public AdminController(TableStorageService tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IActionResult> Dashboard()
        {
            var products =
                await _tableStorage.GetProductsAsync();

            var customers =
                await _tableStorage.GetCustomersAsync();

            ViewBag.ProductCount = products.Count;
            ViewBag.CustomerCount = customers.Count;

            return View();
        }
    }
}
