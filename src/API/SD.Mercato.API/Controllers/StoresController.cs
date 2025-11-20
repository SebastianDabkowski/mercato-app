using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.SellerPanel.DTOs;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for seller store management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IStoreService storeService, ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new store for the authenticated user (seller onboarding).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StoreResponse>> CreateStore([FromBody] CreateStoreRequest request)
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

        var result = await _storeService.CreateStoreAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Store created: {StoreName} for user {UserId}", request.StoreName, userId);
        return Ok(result);
    }

    /// <summary>
    /// Update the authenticated user's store profile.
    /// </summary>
    [HttpPut("{storeId:guid}")]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> UpdateStoreProfile(Guid storeId, [FromBody] UpdateStoreProfileRequest request)
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

        var result = await _storeService.UpdateStoreProfileAsync(storeId, userId, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Store updated: {StoreId} by user {UserId}", storeId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get the authenticated user's store.
    /// </summary>
    [HttpGet("my-store")]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreDto>> GetMyStore()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var store = await _storeService.GetStoreByOwnerIdAsync(userId);

        if (store == null)
        {
            return NotFound(new { message = "Store not found" });
        }

        return Ok(store);
    }

    /// <summary>
    /// Get a store by ID (owner only).
    /// </summary>
    [HttpGet("{storeId:guid}")]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreDto>> GetStoreById(Guid storeId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var store = await _storeService.GetStoreByIdAsync(storeId);

        if (store == null)
        {
            return NotFound(new { message = "Store not found" });
        }

        // Only allow the store owner to view sensitive store details
        if (store.OwnerUserId != userId)
        {
            return Forbid();
        }

        return Ok(store);
    }

    /// <summary>
    /// Check if a store name is available.
    /// </summary>
    [HttpGet("check-name/{storeName}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> CheckStoreName(string storeName)
    {
        var isAvailable = await _storeService.IsStoreNameAvailableAsync(storeName);
        return Ok(new { storeName, isAvailable });
    }

    /// <summary>
    /// Get public store profile by store name (for buyer-facing store page).
    /// </summary>
    [HttpGet("public/{storeName}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicStoreProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicStoreProfileDto>> GetPublicStoreProfile(string storeName)
    {
        var store = await _storeService.GetPublicStoreProfileByNameAsync(storeName);

        if (store == null)
        {
            return NotFound(new { message = "Store not found" });
        }

        return Ok(store);
    }
}
