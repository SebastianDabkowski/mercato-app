using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Reviews.DTOs;

/// <summary>
/// Request to create a seller review.
/// </summary>
public class CreateReviewRequest
{
    [Required(ErrorMessage = "SubOrder ID is required")]
    public Guid SubOrderId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    public string? Comment { get; set; }
}

/// <summary>
/// Request to create a product review.
/// </summary>
public class CreateProductReviewRequest
{
    [Required(ErrorMessage = "SubOrderItem ID is required")]
    public Guid SubOrderItemId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    public string? Comment { get; set; }
}

/// <summary>
/// Response for review operations.
/// </summary>
public class ReviewResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ReviewDto? Review { get; set; }
}

/// <summary>
/// Response for product review operations.
/// </summary>
public class ProductReviewResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ProductReviewDto? Review { get; set; }
}

/// <summary>
/// Seller review DTO.
/// </summary>
public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid SubOrderId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Product review DTO.
/// </summary>
public class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid SubOrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to moderate a review (admin only).
/// </summary>
public class ModerateReviewRequest
{
    [Required(ErrorMessage = "Action is required")]
    [RegularExpression("(?i)^(hide|approve|delete)$", ErrorMessage = "Action must be 'hide', 'approve', or 'delete'")]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
    public string? Note { get; set; }
}

/// <summary>
/// Rating statistics for a store.
/// </summary>
public class StoreRatingStats
{
    public Guid StoreId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
}

/// <summary>
/// Rating statistics for a product.
/// </summary>
public class ProductRatingStats
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
}

/// <summary>
/// Paginated list of reviews.
/// </summary>
public class ReviewListResponse
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>
/// Paginated list of product reviews.
/// </summary>
public class ProductReviewListResponse
{
    public List<ProductReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
