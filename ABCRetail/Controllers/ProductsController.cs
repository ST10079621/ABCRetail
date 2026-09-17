using ABCRetail.Models;
using ABCRetail.Services.AzureFunctions;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ABCRetail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tableStorage;
        private readonly AzureFunctionsService _azureFunctionsService;

        public ProductsController(
            TableStorageService tableStorage,
            AzureFunctionsService azureFunctionsService)
        {
            _tableStorage = tableStorage;
            _azureFunctionsService = azureFunctionsService;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products =
                await _tableStorage.GetProductsAsync();

            return View(products);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            product.PartitionKey = "Products";

            await _azureFunctionsService.PostJsonAsync(
                "StoreCustomerFunction",
                new
                {
                    type = "product",
                    product.PartitionKey,
                    product.RowKey,
                    product.ProductName,
                    product.Category,
                    product.Price,
                    product.Quantity
                });

            TempData["SuccessMessage"] =
                "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var product =
                await _tableStorage.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Products/Edit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            await _tableStorage.UpdateProductAsync(product);

            TempData["SuccessMessage"] =
                "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var product =
                await _tableStorage.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Products/Delete
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            await _tableStorage.DeleteProductAsync(id);

            TempData["SuccessMessage"] =
                "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}