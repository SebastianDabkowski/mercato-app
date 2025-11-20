using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SD.Mercato.Cart.Data;

/// <summary>
/// Design-time factory for CartDbContext.
/// Used by EF Core tools for migrations.
/// </summary>
public class CartDbContextFactory : IDesignTimeDbContextFactory<CartDbContext>
{
    public CartDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CartDbContext>();
        
        // Use connection string for migrations
        // The actual connection string is provided at runtime from appsettings.json
        // For design-time (migrations), the connection string can be overridden via
        // MERCATO_CART_MIGRATION_CONNECTION_STRING environment variable
        var connectionString = Environment.GetEnvironmentVariable("MERCATO_CART_MIGRATION_CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MercatoDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new CartDbContext(optionsBuilder.Options);
    }
}
