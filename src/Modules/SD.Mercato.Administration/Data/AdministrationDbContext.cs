using Microsoft.EntityFrameworkCore;
using SD.Mercato.Administration.Models;

namespace SD.Mercato.Administration.Data;

/// <summary>
/// Database context for the Administration module.
/// </summary>
public class AdministrationDbContext : DbContext
{
    public AdministrationDbContext(DbContextOptions<AdministrationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Admin audit logs.
    /// </summary>
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AdminAuditLog entity
        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.ToTable("AdminAuditLogs");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.AdminUserId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.PerformedAt);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });
    }
}
