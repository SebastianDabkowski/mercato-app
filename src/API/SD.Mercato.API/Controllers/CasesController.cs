using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Models;
using SD.Mercato.History.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for case management (returns and complaints).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly ICaseService _caseService;
    private readonly ILogger<CasesController> _logger;

    public CasesController(ICaseService caseService, ILogger<CasesController> logger)
    {
        _caseService = caseService;
        _logger = logger;
    }

    /// <summary>
    /// Create a return request (Buyer only).
    /// </summary>
    [HttpPost("return")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(CreateCaseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateCaseResponseDto>> CreateReturnRequest([FromBody] CreateReturnRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            var response = await _caseService.CreateReturnRequestAsync(userId, request);
            return CreatedAtAction(nameof(GetCaseById), new { id = response.CaseId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a complaint (Buyer only).
    /// </summary>
    [HttpPost("complaint")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(CreateCaseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateCaseResponseDto>> CreateComplaint([FromBody] CreateComplaintRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            var response = await _caseService.CreateComplaintAsync(userId, request);
            return CreatedAtAction(nameof(GetCaseById), new { id = response.CaseId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all cases for the authenticated buyer.
    /// </summary>
    [HttpGet("buyer")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(CaseListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseListResponseDto>> GetBuyerCases(
        [FromQuery] string? status = null,
        [FromQuery] string? caseType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Validate pagination
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        // Validate status
        if (!string.IsNullOrEmpty(status))
        {
            var validStatuses = new[] { CaseStatuses.New, CaseStatuses.InReview, CaseStatuses.Accepted, CaseStatuses.Rejected, CaseStatuses.Resolved };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { message = $"Invalid status. Valid values are: {string.Join(", ", validStatuses)}" });
            }
        }

        // Validate case type
        if (!string.IsNullOrEmpty(caseType))
        {
            var validTypes = new[] { CaseTypes.Return, CaseTypes.Complaint };
            if (!validTypes.Contains(caseType))
            {
                return BadRequest(new { message = $"Invalid case type. Valid values are: {string.Join(", ", validTypes)}" });
            }
        }

        var filter = new CaseFilterRequestDto
        {
            Status = status,
            CaseType = caseType,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var response = await _caseService.GetCasesForBuyerAsync(userId, filter);
        return Ok(response);
    }

    /// <summary>
    /// Get all cases for a seller's store.
    /// </summary>
    [HttpGet("seller")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(CaseListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseListResponseDto>> GetSellerCases(
        [FromQuery] string? status = null,
        [FromQuery] string? caseType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Get seller's storeId from authenticated user/claims
        // For now, we need a way to map seller userId to storeId
        // This should be done via a store lookup service

        return BadRequest(new { message = "Seller store lookup not yet implemented. Use /api/cases/seller/{storeId} instead." });
    }

    /// <summary>
    /// Get all cases for a specific seller's store (alternative endpoint with explicit storeId).
    /// </summary>
    [HttpGet("seller/{storeId:guid}")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(CaseListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseListResponseDto>> GetSellerCasesByStore(
        Guid storeId,
        [FromQuery] string? status = null,
        [FromQuery] string? caseType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Verify that the authenticated seller owns this storeId
        // For now, we trust the seller role authorization

        // Validate pagination
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var filter = new CaseFilterRequestDto
        {
            Status = status,
            CaseType = caseType,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var response = await _caseService.GetCasesForSellerAsync(storeId, filter);
        return Ok(response);
    }

    /// <summary>
    /// Get all cases (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(CaseListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseListResponseDto>> GetAllCases(
        [FromQuery] string? status = null,
        [FromQuery] string? caseType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate pagination
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var filter = new CaseFilterRequestDto
        {
            Status = status,
            CaseType = caseType,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var response = await _caseService.GetAllCasesAsync(filter);
        return Ok(response);
    }

    /// <summary>
    /// Get case details by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseDto>> GetCaseById(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var caseDto = await _caseService.GetCaseByIdAsync(id);
        if (caseDto == null)
        {
            return NotFound(new { message = "Case not found" });
        }

        // TODO: Verify authorization - buyer should only see their cases, seller their store's cases, admin all
        // For MVP, we allow any authenticated user to view any case

        return Ok(caseDto);
    }

    /// <summary>
    /// Update case status (Seller or Admin).
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Seller,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCaseStatus(Guid id, [FromBody] UpdateCaseStatusRequestDto request)
    {
        // TODO: Verify that seller owns the store related to this case
        // For MVP, we trust the role authorization

        var (success, errorMessage) = await _caseService.UpdateCaseStatusAsync(id, request);
        
        if (!success)
        {
            if (errorMessage == "Case not found")
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Case status updated successfully" });
    }

    /// <summary>
    /// Add a message to a case.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCaseMessage(Guid id, [FromBody] AddCaseMessageRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // TODO: Verify that user is authorized to add message to this case
        // (buyer owns case, seller owns store, or admin)

        var (success, errorMessage) = await _caseService.AddCaseMessageAsync(id, userId, userName, userRole, request);
        
        if (!success)
        {
            if (errorMessage == "Case not found")
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Message added successfully" });
    }
}
