using Microsoft.EntityFrameworkCore;
using SD.Mercato.ProductCatalog.Models;

namespace SD.Mercato.ProductCatalog.Data;

/// <summary>
/// Database context for the ProductCatalog module.
/// Manages products, categories, and catalog-related entities.
/// </summary>
public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Products collection.
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Categories collection.
    /// </summary>
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure schema name for product catalog module tables
        builder.HasDefaultSchema("productcatalog");

        // Configure Category entity
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.HasIndex(c => c.Name)
                .IsUnique();

            // Self-referencing relationship for parent category
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Product entity
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(p => p.StockQuantity)
                .IsRequired();

            entity.Property(p => p.Weight)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.Length)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.Width)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.Height)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.ImageUrls)
                .HasMaxLength(2000);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(ProductStatus.Draft);

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            // Create composite unique index on StoreId and SKU
            entity.HasIndex(p => new { p.StoreId, p.SKU })
                .IsUnique();

            // Create index on CategoryId
            entity.HasIndex(p => p.CategoryId);

            // Create index on Status for filtering
            entity.HasIndex(p => p.Status);

            // Create index on StoreId for filtering by store
            entity.HasIndex(p => p.StoreId);

            // Configure relationship with Category
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Foreign key to Store in SellerPanel module
            // The actual FK constraint will be added at the database level or when modules are integrated
        });
    }
}
