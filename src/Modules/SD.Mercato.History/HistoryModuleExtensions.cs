using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.History.Data;
using SD.Mercato.History.Services;

namespace SD.Mercato.History;

/// <summary>
/// Extension methods for configuring History module services.
/// </summary>
public static class HistoryModuleExtensions
{
    /// <summary>
    /// Add History module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddHistoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<HistoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("HistoryConnection") 
                ?? configuration.GetConnectionString("DefaultConnection")));

        // Register services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICaseService, CaseService>();

        return services;
    }
}
