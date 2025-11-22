using Microsoft.EntityFrameworkCore;
using SD.Mercato.Notification.Models;

namespace SD.Mercato.Notification.Data;

/// <summary>
/// Database context for the Notification module.
/// </summary>
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Notification logs.
    /// </summary>
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure NotificationLog
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("NotificationLogs");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.RecipientUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.RelatedEntityId, e.RelatedEntityType });
        });
    }
}
