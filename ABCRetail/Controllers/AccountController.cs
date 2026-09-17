using ABCRetail.Models;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ABCRetail.Services.AzureFunctions;

namespace ABCRetail.Controllers
{
    public class AccountController : Controller
    {
        private readonly TableStorageService _tableStorage;
        private readonly AzureFunctionsService _azureFunctionsService;
        private readonly PasswordHasher<Customer> _passwordHasher;

        public AccountController(
            TableStorageService tableStorage,
            AzureFunctionsService azureFunctionsService)
        {
            _tableStorage = tableStorage;
            _azureFunctionsService = azureFunctionsService;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingCustomer =
                await _tableStorage.GetCustomerByEmailAsync(model.Email);

            if (existingCustomer != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

            var customer = new Customer
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Role = "Customer"
            };

            customer.PasswordHash =
                _passwordHasher.HashPassword(
                    customer,
                    model.Password);

            await _azureFunctionsService.PostJsonAsync(
             "StoreCustomerFunction",
             new
                {
                 type = "customer",
                 customer.PartitionKey,
                 customer.RowKey,
                 customer.Name,
                 customer.Email,
                 customer.Phone,
                 customer.PasswordHash,
                 customer.Role
                });



            TempData["SuccessMessage"] =
                "Registration successful. Please log in.";

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer =
                await _tableStorage.GetCustomerByEmailAsync(model.Email);

            if (customer == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    customer,
                    customer.PasswordHash,
                    model.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    customer.Name),

                new Claim(
                    ClaimTypes.Email,
                    customer.Email),

                new Claim(
                    ClaimTypes.Role,
                    customer.Role),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    customer.RowKey)
            };

            var identity = new ClaimsIdentity(
                claims,
                "ABCRetailCookie");

            var principal =
                new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "ABCRetailCookie",
                principal);

            if (customer.Role == "Admin")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            return RedirectToAction(
                "Index",
                "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                "ABCRetailCookie");

            return RedirectToAction("Login");
        }
    }
}

