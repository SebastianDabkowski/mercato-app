using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.SellerPanel.Services;
using SD.Mercato.Users.DTOs;
using SD.Mercato.Users.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for managing API tokens for partner integrations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiTokensController : ControllerBase
{
    private readonly IApiTokenService _apiTokenService;
    private readonly IStoreService _storeService;
    private readonly ILogger<ApiTokensController> _logger;

    public ApiTokensController(
        IApiTokenService apiTokenService,
        IStoreService storeService,
        ILogger<ApiTokensController> logger)
    {
        _apiTokenService = apiTokenService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new API token for the authenticated seller.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApiTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateApiTokenResponse>> CreateToken([FromBody] CreateApiTokenRequest request)
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

        // Get the user's store (API tokens are scoped to sellers)
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return StatusCode(403, new { message = "You must create a store before generating API tokens" });
        }

        var token = await _apiTokenService.CreateTokenAsync(userId, store.Id, request);

        _logger.LogInformation("API token created: {TokenId} for user {UserId}", token.Id, userId);

        return CreatedAtAction(nameof(GetToken), new { tokenId = token.Id }, token);
    }

    /// <summary>
    /// Get all API tokens for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ApiTokenDto>>> GetTokens()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var tokens = await _apiTokenService.GetUserTokensAsync(userId);
        return Ok(tokens);
    }

    /// <summary>
    /// Get a specific API token by ID.
    /// </summary>
    [HttpGet("{tokenId:guid}")]
    [ProducesResponseType(typeof(ApiTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiTokenDto>> GetToken(Guid tokenId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var token = await _apiTokenService.GetTokenByIdAsync(tokenId, userId);
        if (token == null)
        {
            return NotFound(new { message = "Token not found" });
        }

        return Ok(token);
    }

    /// <summary>
    /// Update an API token (name, active status, notes).
    /// </summary>
    [HttpPut("{tokenId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateToken(Guid tokenId, [FromBody] UpdateApiTokenRequest request)
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

        var updated = await _apiTokenService.UpdateTokenAsync(tokenId, userId, request);
        if (!updated)
        {
            return NotFound(new { message = "Token not found" });
        }

        _logger.LogInformation("API token updated: {TokenId} by user {UserId}", tokenId, userId);

        return Ok(new { message = "Token updated successfully" });
    }

    /// <summary>
    /// Delete (revoke) an API token.
    /// </summary>
    [HttpDelete("{tokenId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteToken(Guid tokenId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var deleted = await _apiTokenService.DeleteTokenAsync(tokenId, userId);
        if (!deleted)
        {
            return NotFound(new { message = "Token not found" });
        }

        _logger.LogInformation("API token deleted: {TokenId} by user {UserId}", tokenId, userId);

        return NoContent();
    }
}
