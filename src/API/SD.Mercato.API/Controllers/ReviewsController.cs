using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Reviews.DTOs;
using SD.Mercato.Reviews.Services;
using SD.Mercato.History.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for managing reviews and ratings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly IOrderService _orderService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IReviewService reviewService,
        IOrderService orderService,
        ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _orderService = orderService;
        _logger = logger;
    }

    #region Seller Reviews

    /// <summary>
    /// Create a review for a seller (buyer only, after receiving order).
    /// </summary>
    [HttpPost("sellers")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReviewResponse>> CreateReview([FromBody] CreateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

        var result = await _reviewService.CreateReviewAsync(userId, userName, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Seller review created: {ReviewId} by user {UserId}", result.Review?.Id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a specific seller/store.
    /// </summary>
    [HttpGet("sellers/{storeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReviewListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewListResponse>> GetStoreReviews(
        Guid storeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetReviewsByStoreIdAsync(storeId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get rating statistics for a specific seller/store.
    /// </summary>
    [HttpGet("sellers/{storeId:guid}/stats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StoreRatingStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreRatingStats>> GetStoreRatingStats(Guid storeId)
    {
        var stats = await _reviewService.GetStoreRatingStatsAsync(storeId);
        return Ok(stats);
    }

    /// <summary>
    /// Moderate a seller review (admin only).
    /// </summary>
    [HttpPut("sellers/{reviewId:guid}/moderate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModerateReview(
        Guid reviewId,
        [FromBody] ModerateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var success = await _reviewService.ModerateReviewAsync(reviewId, userId, request);

        if (!success)
        {
            return NotFound(new { message = "Review not found" });
        }

        _logger.LogInformation("Review {ReviewId} moderated by admin {UserId} - Action: {Action}", 
            reviewId, userId, request.Action);

        return Ok(new { message = "Review moderated successfully" });
    }

    #endregion

    #region Product Reviews

    /// <summary>
    /// Create a review for a product (buyer only, after receiving order).
    /// </summary>
    [HttpPost("products")]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductReviewResponse>> CreateProductReview([FromBody] CreateProductReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

        var result = await _reviewService.CreateProductReviewAsync(userId, userName, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Product review created: {ReviewId} by user {UserId}", result.Review?.Id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a specific product.
    /// </summary>
    [HttpGet("products/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductReviewListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductReviewListResponse>> GetProductReviews(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetProductReviewsByProductIdAsync(productId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get rating statistics for a specific product.
    /// </summary>
    [HttpGet("products/{productId:guid}/stats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductRatingStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductRatingStats>> GetProductRatingStats(Guid productId)
    {
        var stats = await _reviewService.GetProductRatingStatsAsync(productId);
        return Ok(stats);
    }

    /// <summary>
    /// Moderate a product review (admin only).
    /// </summary>
    [HttpPut("products/{reviewId:guid}/moderate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModerateProductReview(
        Guid reviewId,
        [FromBody] ModerateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var success = await _reviewService.ModerateProductReviewAsync(reviewId, userId, request);

        if (!success)
        {
            return NotFound(new { message = "Product review not found" });
        }

        _logger.LogInformation("Product review {ReviewId} moderated by admin {UserId} - Action: {Action}", 
            reviewId, userId, request.Action);

        return Ok(new { message = "Product review moderated successfully" });
    }

    #endregion

    #region User's Reviews

    /// <summary>
    /// Get all reviews created by the authenticated user.
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReviewListResponse>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetReviewsByBuyerAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all product reviews created by the authenticated user.
    /// </summary>
    [HttpGet("my-product-reviews")]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductReviewListResponse>> GetMyProductReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetProductReviewsByBuyerAsync(userId, page, pageSize);
        return Ok(result);
    }

    #endregion

    #region Admin

    /// <summary>
    /// Get all reviews for moderation (admin only).
    /// </summary>
    [HttpGet("admin/seller-reviews")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ReviewListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReviewListResponse>> GetAllReviewsForModeration(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetAllReviewsForModerationAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all product reviews for moderation (admin only).
    /// </summary>
    [HttpGet("admin/product-reviews")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ProductReviewListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductReviewListResponse>> GetAllProductReviewsForModeration(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _reviewService.GetAllProductReviewsForModerationAsync(status, page, pageSize);
        return Ok(result);
    }

    #endregion
}
