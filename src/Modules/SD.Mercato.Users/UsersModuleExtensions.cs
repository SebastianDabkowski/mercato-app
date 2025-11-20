using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SD.Mercato.Users.Data;
using SD.Mercato.Users.Models;
using SD.Mercato.Users.Services;
using System.Text;

namespace SD.Mercato.Users;

/// <summary>
/// Extension methods for configuring Users module services.
/// </summary>
public static class UsersModuleExtensions
{
    /// <summary>
    /// Adds Users module services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // For MVP, we auto-confirm emails
        })
        .AddEntityFrameworkStores<UsersDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew for distributed environments
            };
        });

        // Conditionally add OAuth providers only if credentials are configured
        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret) &&
            googleClientId != "your-google-client-id" && googleClientSecret != "your-google-client-secret")
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
        }

        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret) &&
            facebookAppId != "your-facebook-app-id" && facebookAppSecret != "your-facebook-app-secret")
        {
            authBuilder.AddFacebook(options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
            });
        }

        // Add Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireBuyerRole", policy => policy.RequireRole(RoleNames.Buyer));
            options.AddPolicy("RequireSellerRole", policy => policy.RequireRole(RoleNames.Seller));
            options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(RoleNames.Administrator));
        });

        // Add services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Seeds default roles in the database.
    /// </summary>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roles = new[]
        {
            new ApplicationRole { Name = RoleNames.Buyer, Description = "Can browse catalog, manage own account, place orders, view their order history" },
            new ApplicationRole { Name = RoleNames.Seller, Description = "Can manage their own store, products, orders, and financial reports" },
            new ApplicationRole { Name = RoleNames.Administrator, Description = "Can manage the entire platform with access to global configuration and all reports" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }
}
