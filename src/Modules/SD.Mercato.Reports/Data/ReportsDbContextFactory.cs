using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SD.Mercato.Reports.Data;

/// <summary>
/// Factory for creating ReportsDbContext instances at design time.
/// Required for EF Core migrations.
/// </summary>
public class ReportsDbContextFactory : IDesignTimeDbContextFactory<ReportsDbContext>
{
    public ReportsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportsDbContext>();
        
        // Default connection string for migrations
        // This will be overridden at runtime by appsettings.json
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=MercatoReports;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ReportsDbContext(optionsBuilder.Options);
    }
}
