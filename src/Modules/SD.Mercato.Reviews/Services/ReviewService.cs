using Microsoft.EntityFrameworkCore;
using SD.Mercato.Reviews.Data;
using SD.Mercato.Reviews.DTOs;
using SD.Mercato.Reviews.Models;
using SD.Mercato.History.Data;
using SD.Mercato.SellerPanel.Data;
using SD.Mercato.ProductCatalog.Data;

namespace SD.Mercato.Reviews.Services;

/// <summary>
/// Service implementation for managing reviews and ratings.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly ReviewsDbContext _context;
    private readonly HistoryDbContext _historyContext;
    private readonly SellerPanelDbContext _sellerContext;
    private readonly ProductCatalogDbContext _productContext;

    public ReviewService(
        ReviewsDbContext context,
        HistoryDbContext historyContext,
        SellerPanelDbContext sellerContext,
        ProductCatalogDbContext productContext)
    {
        _context = context;
        _historyContext = historyContext;
        _sellerContext = sellerContext;
        _productContext = productContext;
    }

    #region Seller Reviews

    public async Task<ReviewResponse> CreateReviewAsync(string buyerUserId, string buyerName, CreateReviewRequest request)
    {
        // Validate that the SubOrder exists
        var subOrder = await _historyContext.SubOrders
            .Include(so => so.Order)
            .FirstOrDefaultAsync(so => so.Id == request.SubOrderId);

        if (subOrder == null)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "SubOrder not found."
            };
        }

        // Validate that the SubOrder belongs to the buyer
        if (subOrder.Order?.UserId != buyerUserId)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "You can only review your own orders."
            };
        }

        // Validate that the SubOrder is delivered or completed
        if (subOrder.Status != "Delivered" && subOrder.Status != "Completed")
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "You can only review orders that have been delivered."
            };
        }

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
            StoreId = subOrder.StoreId,
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

        var reviewDto = await MapToReviewDtoAsync(review);

        return new ReviewResponse
        {
            Success = true,
            Message = "Review created successfully.",
            Review = reviewDto
        };
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        return review != null ? await MapToReviewDtoAsync(review) : null;
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

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToReviewDtoAsync(review));
        }

        return new ReviewListResponse
        {
            Reviews = reviewDtos,
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

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToReviewDtoAsync(review));
        }

        return new ReviewListResponse
        {
            Reviews = reviewDtos,
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
        // Validate that the SubOrderItem exists
        var subOrderItem = await _historyContext.SubOrderItems
            .Include(soi => soi.SubOrder)
            .ThenInclude(so => so!.Order)
            .FirstOrDefaultAsync(soi => soi.Id == request.SubOrderItemId);

        if (subOrderItem == null)
        {
            return new ProductReviewResponse
            {
                Success = false,
                Message = "Order item not found."
            };
        }

        // Validate that the order belongs to the buyer
        if (subOrderItem.SubOrder?.Order?.UserId != buyerUserId)
        {
            return new ProductReviewResponse
            {
                Success = false,
                Message = "You can only review products you have purchased."
            };
        }

        // Validate that the order is delivered or completed
        if (subOrderItem.SubOrder?.Status != "Delivered" && subOrderItem.SubOrder?.Status != "Completed")
        {
            return new ProductReviewResponse
            {
                Success = false,
                Message = "You can only review products from delivered orders."
            };
        }

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
            ProductId = subOrderItem.ProductId,
            StoreId = subOrderItem.SubOrder!.StoreId,
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

        var reviewDto = await MapToProductReviewDtoAsync(productReview);

        return new ProductReviewResponse
        {
            Success = true,
            Message = "Product review created successfully.",
            Review = reviewDto
        };
    }

    public async Task<ProductReviewDto?> GetProductReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(pr => pr.Id == reviewId);

        return review != null ? await MapToProductReviewDtoAsync(review) : null;
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

        var reviewDtos = new List<ProductReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToProductReviewDtoAsync(review));
        }

        return new ProductReviewListResponse
        {
            Reviews = reviewDtos,
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

        var reviewDtos = new List<ProductReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToProductReviewDtoAsync(review));
        }

        return new ProductReviewListResponse
        {
            Reviews = reviewDtos,
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

        var reviewDtos = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToReviewDtoAsync(review));
        }

        return new ReviewListResponse
        {
            Reviews = reviewDtos,
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

        var reviewDtos = new List<ProductReviewDto>();
        foreach (var review in reviews)
        {
            reviewDtos.Add(await MapToProductReviewDtoAsync(review));
        }

        return new ProductReviewListResponse
        {
            Reviews = reviewDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    #endregion

    #region Helper Methods

    private async Task<ReviewDto> MapToReviewDtoAsync(Review review)
    {
        var storeName = await _sellerContext.Stores
            .Where(s => s.Id == review.StoreId)
            .Select(s => s.DisplayName ?? s.StoreName)
            .FirstOrDefaultAsync() ?? "Unknown Store";

        return new ReviewDto
        {
            Id = review.Id,
            SubOrderId = review.SubOrderId,
            StoreId = review.StoreId,
            StoreName = storeName,
            BuyerName = review.BuyerName,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status,
            IsVisible = review.IsVisible,
            CreatedAt = review.CreatedAt
        };
    }

    private async Task<ProductReviewDto> MapToProductReviewDtoAsync(ProductReview review)
    {
        var productTitle = await _productContext.Products
            .Where(p => p.Id == review.ProductId)
            .Select(p => p.Title)
            .FirstOrDefaultAsync() ?? "Unknown Product";

        var storeName = await _sellerContext.Stores
            .Where(s => s.Id == review.StoreId)
            .Select(s => s.DisplayName ?? s.StoreName)
            .FirstOrDefaultAsync() ?? "Unknown Store";

        return new ProductReviewDto
        {
            Id = review.Id,
            SubOrderItemId = review.SubOrderItemId,
            ProductId = review.ProductId,
            ProductTitle = productTitle,
            StoreId = review.StoreId,
            StoreName = storeName,
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
