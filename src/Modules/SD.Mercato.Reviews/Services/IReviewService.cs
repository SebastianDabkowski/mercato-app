using SD.Mercato.Reviews.DTOs;

namespace SD.Mercato.Reviews.Services;

/// <summary>
/// Service interface for managing reviews and ratings.
/// </summary>
public interface IReviewService
{
    // Seller Reviews
    Task<ReviewResponse> CreateReviewAsync(string buyerUserId, string buyerName, CreateReviewRequest request);
    Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId);
    Task<ReviewListResponse> GetReviewsByStoreIdAsync(Guid storeId, int pageNumber = 1, int pageSize = 10);
    Task<ReviewListResponse> GetReviewsByBuyerAsync(string buyerUserId, int pageNumber = 1, int pageSize = 10);
    Task<StoreRatingStats> GetStoreRatingStatsAsync(Guid storeId);
    Task<bool> ModerateReviewAsync(Guid reviewId, string moderatorUserId, ModerateReviewRequest request);

    // Product Reviews
    Task<ProductReviewResponse> CreateProductReviewAsync(string buyerUserId, string buyerName, CreateProductReviewRequest request);
    Task<ProductReviewDto?> GetProductReviewByIdAsync(Guid reviewId);
    Task<ProductReviewListResponse> GetProductReviewsByProductIdAsync(Guid productId, int pageNumber = 1, int pageSize = 10);
    Task<ProductReviewListResponse> GetProductReviewsByBuyerAsync(string buyerUserId, int pageNumber = 1, int pageSize = 10);
    Task<ProductRatingStats> GetProductRatingStatsAsync(Guid productId);
    Task<bool> ModerateProductReviewAsync(Guid reviewId, string moderatorUserId, ModerateReviewRequest request);

    // Admin
    Task<ReviewListResponse> GetAllReviewsForModerationAsync(string? status = null, int pageNumber = 1, int pageSize = 20);
    Task<ProductReviewListResponse> GetAllProductReviewsForModerationAsync(string? status = null, int pageNumber = 1, int pageSize = 20);
}
