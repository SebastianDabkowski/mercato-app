using Microsoft.EntityFrameworkCore;
using SD.Mercato.Reviews.Models;

namespace SD.Mercato.Reviews.Data;

/// <summary>
/// Database context for the Reviews module.
/// Manages reviews and product reviews.
/// </summary>
public class ReviewsDbContext : DbContext
{
    public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Seller reviews collection.
    /// </summary>
    public DbSet<Review> Reviews { get; set; } = null!;

    /// <summary>
    /// Product reviews collection.
    /// </summary>
    public DbSet<ProductReview> ProductReviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure schema name for reviews module tables
        builder.HasDefaultSchema("reviews");

        // Configure Review entity
        builder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.BuyerUserId)
                .IsRequired()
                .HasMaxLength(450); // Standard Identity user ID length

            entity.Property(r => r.BuyerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(r => r.Rating)
                .IsRequired();

            entity.Property(r => r.Comment)
                .HasMaxLength(2000);

            entity.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(ReviewStatus.Approved);

            entity.Property(r => r.IsVisible)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.Property(r => r.ModeratedByUserId)
                .HasMaxLength(450);

            entity.Property(r => r.ModerationNote)
                .HasMaxLength(500);

            // Unique constraint: one review per buyer per SubOrder
            entity.HasIndex(r => new { r.SubOrderId, r.BuyerUserId })
                .IsUnique();

            // Index on StoreId for filtering reviews by seller
            entity.HasIndex(r => r.StoreId);

            // Index on BuyerUserId for finding a user's reviews
            entity.HasIndex(r => r.BuyerUserId);

            // Index on Status for filtering
            entity.HasIndex(r => r.Status);

            // Index on IsVisible for public queries
            entity.HasIndex(r => r.IsVisible);
        });

        // Configure ProductReview entity
        builder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(pr => pr.Id);

            entity.Property(pr => pr.BuyerUserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(pr => pr.BuyerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(pr => pr.Rating)
                .IsRequired();

            entity.Property(pr => pr.Comment)
                .HasMaxLength(2000);

            entity.Property(pr => pr.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(ReviewStatus.Approved);

            entity.Property(pr => pr.IsVisible)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(pr => pr.CreatedAt)
                .IsRequired();

            entity.Property(pr => pr.ModeratedByUserId)
                .HasMaxLength(450);

            entity.Property(pr => pr.ModerationNote)
                .HasMaxLength(500);

            // Unique constraint: one review per buyer per SubOrderItem
            entity.HasIndex(pr => new { pr.SubOrderItemId, pr.BuyerUserId })
                .IsUnique();

            // Index on ProductId for filtering reviews by product
            entity.HasIndex(pr => pr.ProductId);

            // Index on StoreId for filtering
            entity.HasIndex(pr => pr.StoreId);

            // Index on BuyerUserId for finding a user's reviews
            entity.HasIndex(pr => pr.BuyerUserId);

            // Index on Status for filtering
            entity.HasIndex(pr => pr.Status);

            // Index on IsVisible for public queries
            entity.HasIndex(pr => pr.IsVisible);
        });
    }
}
