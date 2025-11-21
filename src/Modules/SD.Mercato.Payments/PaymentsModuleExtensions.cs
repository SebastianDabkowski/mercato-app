using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Payments.Data;
using SD.Mercato.Payments.Gateways;
using SD.Mercato.Payments.Services;

namespace SD.Mercato.Payments;

/// <summary>
/// Extension methods for configuring the Payments module.
/// </summary>
public static class PaymentsModuleExtensions
{
    /// <summary>
    /// Adds the Payments module services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("SD.Mercato.Payments")));

        // Register payment gateway
        // TODO: Replace MockPaymentGateway with actual gateway implementation (Stripe, PayU, etc.)
        // based on configuration in production
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        // Register services
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
