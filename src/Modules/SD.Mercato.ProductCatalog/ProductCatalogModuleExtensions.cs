using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.Services;

namespace SD.Mercato.ProductCatalog;

/// <summary>
/// Extension methods for configuring ProductCatalog module services.
/// </summary>
public static class ProductCatalogModuleExtensions
{
    /// <summary>
    /// Adds ProductCatalog module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddProductCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<ProductCatalogDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
