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
            // This requires upgrading Microsoft.OpenApi to version 3.x which includes Models namespace
            // For now, authentication must be tested manually with tools like Postman or curl
            
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
