using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.Models;

/// <summary>
/// Represents a product category in the catalog.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Parent category ID for hierarchical categories.
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Indicates if the category is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the category was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a product in a seller's store.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Store ID that owns this product (foreign key to Store in SellerPanel module).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Stock Keeping Unit - unique product identifier within the store.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Product title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Product description.
    /// </summary>
    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category ID.
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Navigation property to Category.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Product price.
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// Stock quantity available.
    /// </summary>
    [Required]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Product weight in grams (for shipping calculation).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product length in centimeters.
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Product width in centimeters.
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Product height in centimeters.
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Array of image URLs (stored as JSON or separate table).
    /// </summary>
    [MaxLength(2000)]
    public string ImageUrls { get; set; } = string.Empty;

    /// <summary>
    /// Product status: Draft, Published, or Archived.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ProductStatus.Draft;

    /// <summary>
    /// Timestamp when the product was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the product was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product status constants.
/// </summary>
public static class ProductStatus
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Archived = "Archived";
}
