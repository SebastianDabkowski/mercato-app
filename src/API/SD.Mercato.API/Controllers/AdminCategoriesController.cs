using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for admin category management endpoints.
/// </summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Administrator")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IAdminCategoryService _adminCategoryService;
    private readonly ILogger<AdminCategoriesController> _logger;

    public AdminCategoriesController(
        IAdminCategoryService adminCategoryService,
        ILogger<AdminCategoriesController> logger)
    {
        _adminCategoryService = adminCategoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories with admin-specific information.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AdminCategoryDto>>> GetAllCategories()
    {
        var categories = await _adminCategoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get category by ID with admin-specific information.
    /// </summary>
    [HttpGet("{categoryId:guid}")]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryDto>> GetCategoryById(Guid categoryId)
    {
        var category = await _adminCategoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Create a new category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminCategoryDto>> CreateCategory([FromBody] AdminCreateCategoryRequest request)
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

        try
        {
            var category = await _adminCategoryService.CreateCategoryAsync(
                request,
                adminUserId,
                adminEmail,
                ipAddress);

            _logger.LogInformation("Category {CategoryId} created by admin {AdminId}",
                category.Id, adminUserId);

            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.Id }, category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing category.
    /// </summary>
    [HttpPut("{categoryId:guid}")]
    [ProducesResponseType(typeof(AdminCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryDto>> UpdateCategory(Guid categoryId, [FromBody] AdminUpdateCategoryRequest request)
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

        try
        {
            var category = await _adminCategoryService.UpdateCategoryAsync(
                categoryId,
                request,
                adminUserId,
                adminEmail,
                ipAddress);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            _logger.LogInformation("Category {CategoryId} updated by admin {AdminId}",
                categoryId, adminUserId);

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a category (soft delete by deactivation).
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var result = await _adminCategoryService.DeleteCategoryAsync(
                categoryId,
                adminUserId,
                adminEmail,
                ipAddress);

            if (!result)
            {
                return NotFound(new { message = "Category not found" });
            }

            _logger.LogInformation("Category {CategoryId} deleted by admin {AdminId}",
                categoryId, adminUserId);

            return Ok(new { message = "Category deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
