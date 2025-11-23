using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Administration.Data;
using SD.Mercato.Administration.Services;

namespace SD.Mercato.Administration;

/// <summary>
/// Extension methods for registering the Administration module services.
/// </summary>
public static class AdministrationModuleExtensions
{
    /// <summary>
    /// Registers the Administration module services and database context.
    /// </summary>
    public static IServiceCollection AddAdministrationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<AdministrationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("SD.Mercato.Administration")));

        // Register services
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminStoreService, AdminStoreService>();
        services.AddScoped<IAdminCategoryService, AdminCategoryService>();

        return services;
    }
}
