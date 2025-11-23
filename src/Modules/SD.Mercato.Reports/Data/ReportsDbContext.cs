using Microsoft.EntityFrameworkCore;
using SD.Mercato.Reports.Models;

namespace SD.Mercato.Reports.Data;

/// <summary>
/// Database context for the Reports module.
/// Handles commission configuration and invoice generation.
/// </summary>
public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Global commission configuration.
    /// </summary>
    public DbSet<GlobalCommissionConfig> GlobalCommissionConfigs { get; set; } = null!;

    /// <summary>
    /// Seller invoices/statements.
    /// </summary>
    public DbSet<SellerInvoice> SellerInvoices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure GlobalCommissionConfig
        modelBuilder.Entity<GlobalCommissionConfig>(entity =>
        {
            entity.ToTable("GlobalCommissionConfigs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultCommissionRate).HasPrecision(5, 4); // e.g., 0.1500 for 15%
            entity.HasIndex(e => e.IsActive);
        });

        // Configure SellerInvoice
        modelBuilder.Entity<SellerInvoice>(entity =>
        {
            entity.ToTable("SellerInvoices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.StoreId);
            entity.HasIndex(e => new { e.StoreId, e.PeriodStartDate, e.PeriodEndDate });
            
            entity.Property(e => e.TotalGMV).HasPrecision(18, 2);
            entity.Property(e => e.TotalProductValue).HasPrecision(18, 2);
            entity.Property(e => e.TotalShippingFees).HasPrecision(18, 2);
            entity.Property(e => e.TotalCommission).HasPrecision(18, 2);
            entity.Property(e => e.TotalProcessingFees).HasPrecision(18, 2);
            entity.Property(e => e.NetAmountDue).HasPrecision(18, 2);
        });

        // Seed default global commission configuration
        modelBuilder.Entity<GlobalCommissionConfig>().HasData(
            new GlobalCommissionConfig
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DefaultCommissionRate = 0.15m,
                Notes = "Default platform commission rate - 15% on product sales",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
