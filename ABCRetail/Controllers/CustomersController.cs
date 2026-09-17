using ABCRetail.Models;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomersController(
            TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        // GET: /Customers
        public async Task<IActionResult> Index()
        {
            var customers =
                await _tableStorageService.GetCustomersAsync();

            return View(customers);
        }

        // GET: /Customers/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var customers =
                await _tableStorageService.GetCustomersAsync();

            var customer =
                customers.FirstOrDefault(
                    c => c.RowKey == id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: /Customers/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            await _tableStorageService.DeleteCustomerAsync(id);

            TempData["SuccessMessage"] =
                "Customer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
