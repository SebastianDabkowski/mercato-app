using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.Mercato.Reviews.Data;
using SD.Mercato.Reviews.Services;

namespace SD.Mercato.Reviews;

/// <summary>
/// Extension methods for configuring Reviews module services.
/// </summary>
public static class ReviewsModuleExtensions
{
    /// <summary>
    /// Adds Reviews module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddReviewsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<ReviewsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
