using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// Client-side DTOs for reviews (matching API DTOs).
/// </summary>
public class CreateReviewRequest
{
    public Guid SubOrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateProductReviewRequest
{
    public Guid SubOrderItemId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

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

public class ReviewResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ReviewDto? Review { get; set; }
}

public class ProductReviewResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ProductReviewDto? Review { get; set; }
}

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

public class ReviewListResponse
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class ProductReviewListResponse
{
    public List<ProductReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class ModerateReviewRequest
{
    public string Action { get; set; } = string.Empty;
    public string? Note { get; set; }
}

/// <summary>
/// Service for managing reviews and ratings on the client side.
/// </summary>
public class ReviewService
{
    private readonly HttpClient _httpClient;

    public ReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Seller Reviews

    public async Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/reviews/sellers", request);
        return await response.Content.ReadFromJsonAsync<ReviewResponse>() 
            ?? new ReviewResponse { Success = false, Message = "Failed to create review" };
    }

    public async Task<ReviewListResponse> GetStoreReviewsAsync(Guid storeId, int page = 1, int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<ReviewListResponse>(
            $"/api/reviews/sellers/{storeId}?page={page}&pageSize={pageSize}") 
            ?? new ReviewListResponse();
    }

    public async Task<StoreRatingStats> GetStoreRatingStatsAsync(Guid storeId)
    {
        return await _httpClient.GetFromJsonAsync<StoreRatingStats>(
            $"/api/reviews/sellers/{storeId}/stats") 
            ?? new StoreRatingStats();
    }

    #endregion

    #region Product Reviews

    public async Task<ProductReviewResponse> CreateProductReviewAsync(CreateProductReviewRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/reviews/products", request);
        return await response.Content.ReadFromJsonAsync<ProductReviewResponse>() 
            ?? new ProductReviewResponse { Success = false, Message = "Failed to create review" };
    }

    public async Task<ProductReviewListResponse> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<ProductReviewListResponse>(
            $"/api/reviews/products/{productId}?page={page}&pageSize={pageSize}") 
            ?? new ProductReviewListResponse();
    }

    public async Task<ProductRatingStats> GetProductRatingStatsAsync(Guid productId)
    {
        return await _httpClient.GetFromJsonAsync<ProductRatingStats>(
            $"/api/reviews/products/{productId}/stats") 
            ?? new ProductRatingStats();
    }

    #endregion

    #region User Reviews

    public async Task<ReviewListResponse> GetMyReviewsAsync(int page = 1, int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<ReviewListResponse>(
            $"/api/reviews/my-reviews?page={page}&pageSize={pageSize}") 
            ?? new ReviewListResponse();
    }

    public async Task<ProductReviewListResponse> GetMyProductReviewsAsync(int page = 1, int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<ProductReviewListResponse>(
            $"/api/reviews/my-product-reviews?page={page}&pageSize={pageSize}") 
            ?? new ProductReviewListResponse();
    }

    #endregion
}
