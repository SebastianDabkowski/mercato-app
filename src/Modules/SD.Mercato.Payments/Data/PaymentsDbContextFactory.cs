using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SD.Mercato.Payments.Data;

/// <summary>
/// Design-time factory for creating PaymentsDbContext instances.
/// Used by EF Core tools for migrations.
/// </summary>
public class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentsDbContext>();

        // Get connection string from environment variable or use default for development
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MercatoDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly("SD.Mercato.Payments"));

        return new PaymentsDbContext(optionsBuilder.Options);
    }
}
