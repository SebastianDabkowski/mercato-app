using Microsoft.EntityFrameworkCore;
using SD.Mercato.SellerPanel.Models;

namespace SD.Mercato.SellerPanel.Data;

/// <summary>
/// Database context for the SellerPanel module.
/// Manages seller stores and related entities.
/// </summary>
public class SellerPanelDbContext : DbContext
{
    public SellerPanelDbContext(DbContextOptions<SellerPanelDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Stores collection.
    /// </summary>
    public DbSet<Store> Stores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure schema name for seller panel module tables
        builder.HasDefaultSchema("sellerpanel");

        // Configure Store entity
        builder.Entity<Store>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.StoreName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.Description)
                .HasMaxLength(2000);

            entity.Property(s => s.LogoUrl)
                .HasMaxLength(500);

            entity.Property(s => s.ContactEmail)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(s => s.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(s => s.StoreType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(s => s.BusinessName)
                .HasMaxLength(200);

            entity.Property(s => s.TaxId)
                .HasMaxLength(50);

            entity.Property(s => s.AddressLine1)
                .HasMaxLength(200);

            entity.Property(s => s.AddressLine2)
                .HasMaxLength(200);

            entity.Property(s => s.City)
                .HasMaxLength(100);

            entity.Property(s => s.State)
                .HasMaxLength(100);

            entity.Property(s => s.PostalCode)
                .HasMaxLength(20);

            entity.Property(s => s.Country)
                .HasMaxLength(100);

            entity.Property(s => s.BankAccountDetails)
                .HasMaxLength(500);

            entity.Property(s => s.CommissionRate)
                .IsRequired()
                .HasColumnType("decimal(5,4)");

            entity.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(s => s.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(s => s.CreatedAt)
                .IsRequired();

            entity.Property(s => s.DeliveryInfo)
                .HasMaxLength(1000);

            entity.Property(s => s.ReturnInfo)
                .HasMaxLength(1000);

            // Create unique index on StoreName (for URL-friendly names)
            entity.HasIndex(s => s.StoreName)
                .IsUnique();

            // Create index on OwnerUserId (for looking up stores by owner)
            entity.HasIndex(s => s.OwnerUserId);

            // Note: Foreign key to ApplicationUser in Users module
            // The actual FK constraint will be added at the database level or when modules are integrated
        });
    }
}
