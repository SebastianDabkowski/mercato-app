using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for category management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new category (admin only - for MVP, simplified to any authenticated user).
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // TODO: Add admin role check when implementing role-based authorization
        var category = await _categoryService.CreateCategoryAsync(request);

        _logger.LogInformation("Category created: {CategoryId}", category.Id);
        return Ok(category);
    }

    /// <summary>
    /// Get all active categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetActiveCategories()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get a category by ID.
    /// </summary>
    [HttpGet("{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);

        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        return Ok(category);
    }
}
