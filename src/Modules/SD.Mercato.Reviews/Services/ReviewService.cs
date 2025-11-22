using Microsoft.EntityFrameworkCore;
using SD.Mercato.Reviews.Data;
using SD.Mercato.Reviews.DTOs;
using SD.Mercato.Reviews.Models;

namespace SD.Mercato.Reviews.Services;

/// <summary>
/// Service implementation for managing reviews and ratings.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly ReviewsDbContext _context;

    public ReviewService(ReviewsDbContext context)
    {
        _context = context;
    }

    #region Seller Reviews

    public async Task<ReviewResponse> CreateReviewAsync(string buyerUserId, string buyerName, CreateReviewRequest request)
    {
        // TODO: Validate that the SubOrder exists and belongs to the buyer
        // TODO: Validate that the SubOrder is delivered/completed before allowing review
        // This requires access to the History module which may not be directly available here.
        // Consider adding these validations in the controller layer or using a domain event/service.

        // Check if buyer already reviewed this SubOrder
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.SubOrderId == request.SubOrderId && r.BuyerUserId == buyerUserId);

        if (existingReview != null)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "You have already reviewed this order."
            };
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            SubOrderId = request.SubOrderId,
            StoreId = Guid.Empty, // TODO: Get StoreId from SubOrder
            BuyerUserId = buyerUserId,
            BuyerName = buyerName,
            Rating = request.Rating,
            Comment = request.Comment,
            Status = ReviewStatus.Approved,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return new ReviewResponse
        {
            Success = true,
            Message = "Review created successfully.",
            Review = MapToReviewDto(review)
        };
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        return review != null ? MapToReviewDto(review) : null;
    }

    public async Task<ReviewListResponse> GetReviewsByStoreIdAsync(Guid storeId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Reviews
            .Where(r => r.StoreId == storeId && r.IsVisible && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ReviewListResponse
        {
            Reviews = reviews.Select(MapToReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ReviewListResponse> GetReviewsByBuyerAsync(string buyerUserId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Reviews
            .Where(r => r.BuyerUserId == buyerUserId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ReviewListResponse
        {
            Reviews = reviews.Select(MapToReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<StoreRatingStats> GetStoreRatingStatsAsync(Guid storeId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.StoreId == storeId && r.IsVisible && r.Status == ReviewStatus.Approved)
            .ToListAsync();

        if (reviews.Count == 0)
        {
            return new StoreRatingStats
            {
                StoreId = storeId,
                AverageRating = 0,
                TotalReviews = 0
            };
        }

        return new StoreRatingStats
        {
            StoreId = storeId,
            AverageRating = reviews.Average(r => r.Rating),
            TotalReviews = reviews.Count,
            FiveStarCount = reviews.Count(r => r.Rating == 5),
            FourStarCount = reviews.Count(r => r.Rating == 4),
            ThreeStarCount = reviews.Count(r => r.Rating == 3),
            TwoStarCount = reviews.Count(r => r.Rating == 2),
            OneStarCount = reviews.Count(r => r.Rating == 1)
        };
    }

    public async Task<bool> ModerateReviewAsync(Guid reviewId, string moderatorUserId, ModerateReviewRequest request)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        review.UpdatedAt = DateTime.UtcNow;
        review.ModeratedByUserId = moderatorUserId;
        review.ModerationNote = request.Note;

        switch (request.Action.ToLower())
        {
            case "hide":
                review.IsVisible = false;
                review.Status = ReviewStatus.Hidden;
                break;
            case "approve":
                review.IsVisible = true;
                review.Status = ReviewStatus.Approved;
                break;
            case "delete":
                review.Status = ReviewStatus.Deleted;
                review.IsVisible = false;
                break;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Product Reviews

    public async Task<ProductReviewResponse> CreateProductReviewAsync(string buyerUserId, string buyerName, CreateProductReviewRequest request)
    {
        // TODO: Validate that the SubOrderItem exists and belongs to the buyer
        // TODO: Validate that the order is delivered/completed before allowing review
        // TODO: Get ProductId and StoreId from SubOrderItem

        // Check if buyer already reviewed this SubOrderItem
        var existingReview = await _context.ProductReviews
            .FirstOrDefaultAsync(pr => pr.SubOrderItemId == request.SubOrderItemId && pr.BuyerUserId == buyerUserId);

        if (existingReview != null)
        {
            return new ProductReviewResponse
            {
                Success = false,
                Message = "You have already reviewed this product."
            };
        }

        var productReview = new ProductReview
        {
            Id = Guid.NewGuid(),
            SubOrderItemId = request.SubOrderItemId,
            ProductId = Guid.Empty, // TODO: Get from SubOrderItem
            StoreId = Guid.Empty, // TODO: Get from SubOrderItem
            BuyerUserId = buyerUserId,
            BuyerName = buyerName,
            Rating = request.Rating,
            Comment = request.Comment,
            Status = ReviewStatus.Approved,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductReviews.Add(productReview);
        await _context.SaveChangesAsync();

        return new ProductReviewResponse
        {
            Success = true,
            Message = "Product review created successfully.",
            Review = MapToProductReviewDto(productReview)
        };
    }

    public async Task<ProductReviewDto?> GetProductReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(pr => pr.Id == reviewId);

        return review != null ? MapToProductReviewDto(review) : null;
    }

    public async Task<ProductReviewListResponse> GetProductReviewsByProductIdAsync(Guid productId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.ProductReviews
            .Where(pr => pr.ProductId == productId && pr.IsVisible && pr.Status == ReviewStatus.Approved)
            .OrderByDescending(pr => pr.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductReviewListResponse
        {
            Reviews = reviews.Select(MapToProductReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductReviewListResponse> GetProductReviewsByBuyerAsync(string buyerUserId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.ProductReviews
            .Where(pr => pr.BuyerUserId == buyerUserId)
            .OrderByDescending(pr => pr.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductReviewListResponse
        {
            Reviews = reviews.Select(MapToProductReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductRatingStats> GetProductRatingStatsAsync(Guid productId)
    {
        var reviews = await _context.ProductReviews
            .Where(pr => pr.ProductId == productId && pr.IsVisible && pr.Status == ReviewStatus.Approved)
            .ToListAsync();

        if (reviews.Count == 0)
        {
            return new ProductRatingStats
            {
                ProductId = productId,
                AverageRating = 0,
                TotalReviews = 0
            };
        }

        return new ProductRatingStats
        {
            ProductId = productId,
            AverageRating = reviews.Average(pr => pr.Rating),
            TotalReviews = reviews.Count,
            FiveStarCount = reviews.Count(pr => pr.Rating == 5),
            FourStarCount = reviews.Count(pr => pr.Rating == 4),
            ThreeStarCount = reviews.Count(pr => pr.Rating == 3),
            TwoStarCount = reviews.Count(pr => pr.Rating == 2),
            OneStarCount = reviews.Count(pr => pr.Rating == 1)
        };
    }

    public async Task<bool> ModerateProductReviewAsync(Guid reviewId, string moderatorUserId, ModerateReviewRequest request)
    {
        var review = await _context.ProductReviews.FindAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        review.UpdatedAt = DateTime.UtcNow;
        review.ModeratedByUserId = moderatorUserId;
        review.ModerationNote = request.Note;

        switch (request.Action.ToLower())
        {
            case "hide":
                review.IsVisible = false;
                review.Status = ReviewStatus.Hidden;
                break;
            case "approve":
                review.IsVisible = true;
                review.Status = ReviewStatus.Approved;
                break;
            case "delete":
                review.Status = ReviewStatus.Deleted;
                review.IsVisible = false;
                break;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Admin

    public async Task<ReviewListResponse> GetAllReviewsForModerationAsync(string? status = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Reviews.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ReviewListResponse
        {
            Reviews = reviews.Select(MapToReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductReviewListResponse> GetAllProductReviewsForModerationAsync(string? status = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.ProductReviews.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(pr => pr.Status == status);
        }

        query = query.OrderByDescending(pr => pr.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductReviewListResponse
        {
            Reviews = reviews.Select(MapToProductReviewDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    #endregion

    #region Helper Methods

    private static ReviewDto MapToReviewDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            SubOrderId = review.SubOrderId,
            StoreId = review.StoreId,
            StoreName = string.Empty, // TODO: Populate from Store lookup
            BuyerName = review.BuyerName,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status,
            IsVisible = review.IsVisible,
            CreatedAt = review.CreatedAt
        };
    }

    private static ProductReviewDto MapToProductReviewDto(ProductReview review)
    {
        return new ProductReviewDto
        {
            Id = review.Id,
            SubOrderItemId = review.SubOrderItemId,
            ProductId = review.ProductId,
            ProductTitle = string.Empty, // TODO: Populate from Product lookup
            StoreId = review.StoreId,
            StoreName = string.Empty, // TODO: Populate from Store lookup
            BuyerName = review.BuyerName,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status,
            IsVisible = review.IsVisible,
            CreatedAt = review.CreatedAt
        };
    }

    #endregion
}
