using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using SD.Mercato.UI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add logging services
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7147/") 
});

// Add AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Add authorization services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISellerOrderService, SellerOrderService>();
builder.Services.AddScoped<ISellerReportService, SellerReportService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

// Add Admin services
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminStoreService, AdminStoreService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<ISellerDashboardService, SellerDashboardService>();

await builder.Build().RunAsync();
