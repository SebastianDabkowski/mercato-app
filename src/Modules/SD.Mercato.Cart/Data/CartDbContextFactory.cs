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
        
        // Use a temporary connection string for migrations
        // The actual connection string is provided at runtime from appsettings.json
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MercatoDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new CartDbContext(optionsBuilder.Options);
    }
}
