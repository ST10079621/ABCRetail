
using ABCRetail.Services;
using ABCRetail.Services.AzureStorage;
using ABCRetail.Services.AzureFunctions;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSingleton<TableStorageService>();

builder.Services.AddSingleton<BlobStorageService>();

builder.Services.AddSingleton<QueueStorageService>();

builder.Services.AddSingleton<FileStorageService>();

builder.Services.AddScoped<AdminSeeder>();

builder.Services.AddHttpClient<AzureFunctionsService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("ABCRetailCookie")
    .AddCookie("ABCRetailCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var adminSeeder =
        scope.ServiceProvider.GetRequiredService<AdminSeeder>();

    await adminSeeder.SeedAdminAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
