using ABCRetail.Models;
using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Identity;

namespace ABCRetail.Services
{
    public class AdminSeeder
    {
        private readonly TableStorageService _tableStorage;
        private readonly IConfiguration _configuration;

        public AdminSeeder(
            TableStorageService tableStorage,
            IConfiguration configuration)
        {
            _tableStorage = tableStorage;
            _configuration = configuration;
        }

        public async Task SeedAdminAsync()
        {
            var email =
                _configuration["AdminSettings:Email"];

            var password =
                _configuration["AdminSettings:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var existingAdmin =
                await _tableStorage.GetCustomerByEmailAsync(email);

            if (existingAdmin != null)
            {
                if (existingAdmin.Role != "Admin")
                {
                    existingAdmin.Role = "Admin";

                    await _tableStorage.UpdateCustomerAsync(
                        existingAdmin);
                }

                return;
            }

            var admin = new Customer
            {
                Name = "ABC Retail Administrator",
                Email = email,
                Phone = "0000000000",
                Role = "Admin"
            };

            var passwordHasher =
                new PasswordHasher<Customer>();

            admin.PasswordHash =
                passwordHasher.HashPassword(
                    admin,
                    password);

            await _tableStorage.AddCustomerAsync(admin);
        }
    }
}