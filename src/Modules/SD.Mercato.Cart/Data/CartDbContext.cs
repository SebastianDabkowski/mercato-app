using Microsoft.EntityFrameworkCore;
using SD.Mercato.Cart.Models;

namespace SD.Mercato.Cart.Data;

/// <summary>
/// Database context for the Cart module.
/// Manages shopping carts and cart items.
/// </summary>
public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Carts collection.
    /// </summary>
    public DbSet<Models.Cart> Carts { get; set; } = null!;

    /// <summary>
    /// Cart items collection.
    /// </summary>
    public DbSet<CartItem> CartItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure schema name for cart module tables
        builder.HasDefaultSchema("cart");

        // Configure Cart entity
        builder.Entity<Models.Cart>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.UserId)
                .HasMaxLength(450); // Standard ASP.NET Identity user ID length

            entity.Property(c => c.SessionId)
                .HasMaxLength(200);

            entity.Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(CartStatus.Active);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.Property(c => c.LastUpdatedAt)
                .IsRequired();

            // Create index on UserId for fast lookup of user carts
            entity.HasIndex(c => c.UserId);

            // Create index on SessionId for fast lookup of guest carts
            entity.HasIndex(c => c.SessionId);

            // Create index on Status for filtering active carts
            entity.HasIndex(c => c.Status);

            // Create index on LastUpdatedAt for cart expiration queries
            entity.HasIndex(c => c.LastUpdatedAt);
        });

        // Configure CartItem entity
        builder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);

            entity.Property(ci => ci.CartId)
                .IsRequired();

            entity.Property(ci => ci.ProductId)
                .IsRequired();

            entity.Property(ci => ci.StoreId)
                .IsRequired();

            entity.Property(ci => ci.Quantity)
                .IsRequired();

            entity.Property(ci => ci.PriceAtAdd)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(ci => ci.AddedAt)
                .IsRequired();

            // Configure relationship with Cart
            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create index on CartId for fast lookup of cart items
            entity.HasIndex(ci => ci.CartId);

            // Create index on ProductId for fast lookup of items by product
            entity.HasIndex(ci => ci.ProductId);

            // Create composite unique index to prevent duplicate products in same cart
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            // Note: Foreign key to Product in ProductCatalog module
            // The actual FK constraint will be added at the database level or when modules are integrated
        });
    }
}
