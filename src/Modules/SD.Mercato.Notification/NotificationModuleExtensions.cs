using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Notification.Data;
using SD.Mercato.Notification.Services;

namespace SD.Mercato.Notification;

/// <summary>
/// Extension methods for configuring Notification module services.
/// </summary>
public static class NotificationModuleExtensions
{
    /// <summary>
    /// Add Notification module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddNotificationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("NotificationConnection")
                ?? configuration.GetConnectionString("DefaultConnection")));

        // Register services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
