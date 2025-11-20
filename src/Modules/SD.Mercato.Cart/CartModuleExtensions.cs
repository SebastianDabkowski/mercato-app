using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Cart.Data;
using SD.Mercato.Cart.Services;

namespace SD.Mercato.Cart;

/// <summary>
/// Extension methods for configuring the Cart module.
/// </summary>
public static class CartModuleExtensions
{
    /// <summary>
    /// Adds the Cart module services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCartModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<CartDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("SD.Mercato.Cart")));

        // Register services
        services.AddScoped<ICartService, CartService>();

        return services;
    }
}
