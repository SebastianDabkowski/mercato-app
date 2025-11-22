using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.ProductCatalog.Models;

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
        services.AddScoped<IProductQuestionService, ProductQuestionService>();

        return services;
    }

    /// <summary>
    /// Seeds default categories in the database.
    /// </summary>
    public static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

        // Check if categories already exist
        if (await context.Categories.AnyAsync())
        {
            return; // Categories already seeded
        }

        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Clothing", Description = "Apparel and fashion items", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Home & Garden", Description = "Home decor and gardening supplies", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Books", Description = "Books and educational materials", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Toys & Games", Description = "Toys, games, and hobby items", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Health & Beauty", Description = "Health and beauty products", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Food & Beverages", Description = "Food and drink products", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
