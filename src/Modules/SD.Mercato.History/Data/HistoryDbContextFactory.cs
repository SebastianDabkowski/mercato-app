using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SD.Mercato.History.Data;

/// <summary>
/// Factory for creating HistoryDbContext instances at design time (for migrations).
/// </summary>
public class HistoryDbContextFactory : IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HistoryDbContext>();
        
        // Use a connection string for migrations
        // This will be overridden at runtime by the actual configuration
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MercatoHistory;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new HistoryDbContext(optionsBuilder.Options);
    }
}
