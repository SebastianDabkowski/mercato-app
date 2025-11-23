using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Reports.Data;
using SD.Mercato.Reports.Services;

namespace SD.Mercato.Reports;

/// <summary>
/// Extension methods for configuring the Reports module.
/// </summary>
public static class ReportsModuleExtensions
{
    /// <summary>
    /// Adds the Reports module services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ReportsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("SD.Mercato.Reports")));

        // Register services
        services.AddScoped<ISellerReportService, SellerReportService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICommissionConfigService, CommissionConfigService>();

        return services;
    }
}
