using Microsoft.Extensions.DependencyInjection;

namespace SD.Mercato.API.Configuration;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI.
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // TODO: Add Swagger security schemes for API Key and Bearer authentication
            // The required OpenApiInfo, OpenApiSecurityScheme types need Microsoft.OpenApi package
            // with the Models namespace. Current Swashbuckle.AspNetCore transitively includes
            // an older version. For now, authentication must be tested manually with Postman/curl.
            // To add security schemes: install a compatible Microsoft.OpenApi version and configure
            // options.AddSecurityDefinition for "ApiKey" and "Bearer" schemes.
            
            // Enable XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
