using System.ComponentModel.DataAnnotations;
using SD.Mercato.ProductCatalog.Validation;

namespace SD.Mercato.ProductCatalog.DTOs;

/// <summary>
/// Request model for creating a new product.
/// </summary>
public class CreateProductRequest
{
    [Required(ErrorMessage = "SKU is required")]
    [MaxLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
    public decimal? Weight { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Length cannot be negative")]
    public decimal? Length { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Width cannot be negative")]
    public decimal? Width { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Height cannot be negative")]
    public decimal? Height { get; set; }

    [UrlList(ErrorMessage = "One or more image URLs are invalid")]
    public List<string>? ImageUrls { get; set; }

    public string Status { get; set; } = "Draft";
}

/// <summary>
/// Request model for updating a product.
/// </summary>
public class UpdateProductRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
    public decimal? Weight { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Length cannot be negative")]
    public decimal? Length { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Width cannot be negative")]
    public decimal? Width { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Height cannot be negative")]
    public decimal? Height { get; set; }

    [UrlList(ErrorMessage = "One or more image URLs are invalid")]
    public List<string>? ImageUrls { get; set; }

    public string Status { get; set; } = "Draft";
}

/// <summary>
/// Response model for product operations.
/// </summary>
public class ProductResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ProductDto? Product { get; set; }
}

/// <summary>
/// Product data transfer object.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Public product DTO for buyer-facing catalog.
/// </summary>
public class PublicProductDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating a category.
/// </summary>
public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }
}

/// <summary>
/// Category data transfer object.
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
