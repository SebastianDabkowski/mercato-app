using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using SD.Mercato.UI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7147/") 
});

// Add AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoreService, StoreService>();

await builder.Build().RunAsync();
