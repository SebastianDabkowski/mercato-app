using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for admin store/seller management endpoints.
/// </summary>
[ApiController]
[Route("api/admin/stores")]
[Authorize(Roles = "Administrator")]
public class AdminStoresController : ControllerBase
{
    private readonly IAdminStoreService _adminStoreService;
    private readonly ILogger<AdminStoresController> _logger;

    public AdminStoresController(
        IAdminStoreService adminStoreService,
        ILogger<AdminStoresController> logger)
    {
        _adminStoreService = adminStoreService;
        _logger = logger;
    }

    /// <summary>
    /// Search and list stores with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedStoresResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedStoresResponse>> SearchStores([FromQuery] AdminStoreSearchRequest request)
    {
        var result = await _adminStoreService.SearchStoresAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed store information with KPIs.
    /// </summary>
    [HttpGet("{storeId:guid}")]
    [ProducesResponseType(typeof(AdminStoreDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminStoreDetailDto>> GetStoreDetail(Guid storeId)
    {
        var store = await _adminStoreService.GetStoreDetailAsync(storeId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found" });
        }

        return Ok(store);
    }

    /// <summary>
    /// Update store status (active/verified).
    /// </summary>
    [HttpPut("{storeId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStoreStatus(Guid storeId, [FromBody] UpdateStoreStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _adminStoreService.UpdateStoreStatusAsync(
            storeId,
            request,
            adminUserId,
            adminEmail,
            ipAddress);

        if (!result)
        {
            return NotFound(new { message = "Store not found or no changes made" });
        }

        _logger.LogInformation("Store {StoreId} status updated by admin {AdminId}",
            storeId, adminUserId);

        return Ok(new { message = "Store status updated successfully" });
    }

    /// <summary>
    /// Update store commission rate.
    /// </summary>
    [HttpPut("{storeId:guid}/commission")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStoreCommission(Guid storeId, [FromBody] UpdateStoreCommissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _adminStoreService.UpdateStoreCommissionAsync(
            storeId,
            request,
            adminUserId,
            adminEmail,
            ipAddress);

        if (!result)
        {
            return NotFound(new { message = "Store not found" });
        }

        _logger.LogInformation("Store {StoreId} commission rate updated to {Rate} by admin {AdminId}",
            storeId, request.CommissionRate, adminUserId);

        return Ok(new { message = "Store commission rate updated successfully" });
    }
}
