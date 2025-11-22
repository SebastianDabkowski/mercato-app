using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Reviews.Models;

/// <summary>
/// Represents a review and rating for a seller.
/// Linked to a specific SubOrder (one review per buyer per SubOrder).
/// </summary>
public class Review
{
    /// <summary>
    /// Unique identifier for the review.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// SubOrder ID this review is associated with.
    /// </summary>
    [Required]
    public Guid SubOrderId { get; set; }

    /// <summary>
    /// Store ID being reviewed (seller).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// User ID of the buyer who wrote this review.
    /// </summary>
    [Required]
    public string BuyerUserId { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's display name at the time of review.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>
    /// Rating (1-5 stars).
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Optional review comment.
    /// </summary>
    [MaxLength(2000)]
    public string? Comment { get; set; }

    /// <summary>
    /// Review status for moderation.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ReviewStatus.Approved;

    /// <summary>
    /// Indicates whether the review is visible to the public.
    /// </summary>
    [Required]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Timestamp when the review was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the review was last updated (for moderation actions).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID of the moderator who took action (hide/delete).
    /// </summary>
    public string? ModeratedByUserId { get; set; }

    /// <summary>
    /// Moderation reason/note.
    /// </summary>
    [MaxLength(500)]
    public string? ModerationNote { get; set; }
}

/// <summary>
/// Represents a review and rating for a specific product.
/// Linked to a SubOrderItem (one review per buyer per product purchase).
/// </summary>
public class ProductReview
{
    /// <summary>
    /// Unique identifier for the product review.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// SubOrderItem ID this review is associated with.
    /// </summary>
    [Required]
    public Guid SubOrderItemId { get; set; }

    /// <summary>
    /// Product ID being reviewed.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Store ID that owns the product (for reference).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// User ID of the buyer who wrote this review.
    /// </summary>
    [Required]
    public string BuyerUserId { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's display name at the time of review.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>
    /// Rating (1-5 stars).
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Optional review comment.
    /// </summary>
    [MaxLength(2000)]
    public string? Comment { get; set; }

    /// <summary>
    /// Review status for moderation.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ReviewStatus.Approved;

    /// <summary>
    /// Indicates whether the review is visible to the public.
    /// </summary>
    [Required]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Timestamp when the review was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the review was last updated (for moderation actions).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID of the moderator who took action (hide/delete).
    /// </summary>
    public string? ModeratedByUserId { get; set; }

    /// <summary>
    /// Moderation reason/note.
    /// </summary>
    [MaxLength(500)]
    public string? ModerationNote { get; set; }
}

/// <summary>
/// Review status constants for moderation.
/// </summary>
public static class ReviewStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Hidden = "Hidden";
    public const string Deleted = "Deleted";
}
