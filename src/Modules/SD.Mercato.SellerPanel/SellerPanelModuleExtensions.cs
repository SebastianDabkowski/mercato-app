using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.SellerPanel.Data;
using SD.Mercato.SellerPanel.Services;

namespace SD.Mercato.SellerPanel;

/// <summary>
/// Extension methods for configuring SellerPanel module services.
/// </summary>
public static class SellerPanelModuleExtensions
{
    /// <summary>
    /// Adds SellerPanel module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddSellerPanelModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<SellerPanelDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services
        services.AddScoped<IStoreService, StoreService>();

        return services;
    }
}
