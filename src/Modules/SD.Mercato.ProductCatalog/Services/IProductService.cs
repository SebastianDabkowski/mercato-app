using SD.Mercato.ProductCatalog.DTOs;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Interface for product management services.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Create a new product for a store.
    /// </summary>
    Task<ProductResponse> CreateProductAsync(Guid storeId, CreateProductRequest request);

    /// <summary>
    /// Update an existing product.
    /// </summary>
    Task<ProductResponse> UpdateProductAsync(Guid productId, Guid storeId, UpdateProductRequest request);

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    Task<ProductDto?> GetProductByIdAsync(Guid productId);

    /// <summary>
    /// Get all products for a store.
    /// </summary>
    Task<List<ProductDto>> GetProductsByStoreIdAsync(Guid storeId);

    /// <summary>
    /// Get published products for a store (for public catalog).
    /// </summary>
    Task<List<PublicProductDto>> GetPublishedProductsByStoreIdAsync(Guid storeId);

    /// <summary>
    /// Get all published products from all stores (for global catalog).
    /// </summary>
    Task<List<PublicProductDto>> GetAllPublishedProductsAsync();

    /// <summary>
    /// Search and filter published products with pagination.
    /// </summary>
    Task<PaginatedProductsResponse> SearchProductsAsync(ProductSearchRequest request);

    /// <summary>
    /// Delete a product.
    /// </summary>
    Task<bool> DeleteProductAsync(Guid productId, Guid storeId);

    /// <summary>
    /// Check if SKU is available within a store.
    /// </summary>
    Task<bool> IsSKUAvailableAsync(Guid storeId, string sku, Guid? excludeProductId = null);

    /// <summary>
    /// Get multiple products by their IDs (batch load).
    /// </summary>
    Task<List<ProductDto>> GetProductsByIdsAsync(List<Guid> productIds);
}

/// <summary>
/// Interface for category management services.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Create a new category.
    /// </summary>
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);

    /// <summary>
    /// Get all active categories.
    /// </summary>
    Task<List<CategoryDto>> GetActiveCategoriesAsync();

    /// <summary>
    /// Get a category by ID.
    /// </summary>
    Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);
}
