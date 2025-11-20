using Microsoft.EntityFrameworkCore;
using SD.Mercato.History.Models;

namespace SD.Mercato.History.Data;

/// <summary>
/// Database context for the History module (Orders and SubOrders).
/// </summary>
public class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Orders table.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// SubOrders table.
    /// </summary>
    public DbSet<SubOrder> SubOrders => Set<SubOrder>();

    /// <summary>
    /// SubOrderItems table.
    /// </summary>
    public DbSet<SubOrderItem> SubOrderItems => Set<SubOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.CreatedAt);
            entity.HasIndex(o => o.Status);

            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

            // Configure relationship with SubOrders
            entity.HasMany(o => o.SubOrders)
                .WithOne(s => s.Order)
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SubOrder entity
        modelBuilder.Entity<SubOrder>(entity =>
        {
            entity.ToTable("SubOrders");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.SubOrderNumber).IsUnique();
            entity.HasIndex(s => s.OrderId);
            entity.HasIndex(s => s.StoreId);
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.CreatedAt);

            entity.Property(s => s.ProductsTotal).HasPrecision(18, 2);
            entity.Property(s => s.ShippingCost).HasPrecision(18, 2);
            entity.Property(s => s.TotalAmount).HasPrecision(18, 2);

            // Configure relationship with SubOrderItems
            entity.HasMany(s => s.Items)
                .WithOne(i => i.SubOrder)
                .HasForeignKey(i => i.SubOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SubOrderItem entity
        modelBuilder.Entity<SubOrderItem>(entity =>
        {
            entity.ToTable("SubOrderItems");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.SubOrderId);
            entity.HasIndex(i => i.ProductId);

            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.Subtotal).HasPrecision(18, 2);
        });
    }
}
