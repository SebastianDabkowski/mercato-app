using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SD.Mercato.Notification.Data;

/// <summary>
/// Design-time factory for creating NotificationDbContext during migrations.
/// </summary>
public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        // Use a default connection string for migrations
        // In production, this will be overridden by appsettings configuration
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=MercatoNotification;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
