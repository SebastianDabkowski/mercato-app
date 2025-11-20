using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Models;
using System.Text.Json;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Service for managing products.
/// </summary>
public class ProductService : IProductService
{
    private readonly ProductCatalogDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductCatalogDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductResponse> CreateProductAsync(Guid storeId, CreateProductRequest request)
    {
        try
        {
            // Check if SKU is available
            if (!await IsSKUAvailableAsync(storeId, request.SKU))
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "SKU already exists in this store. Please use a different SKU."
                };
            }

            // Validate category exists and is active
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null || !category.IsActive)
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Invalid or inactive category."
                };
            }

            // Validate status
            if (request.Status != ProductStatus.Draft && 
                request.Status != ProductStatus.Published && 
                request.Status != ProductStatus.Archived)
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Invalid product status. Must be 'Draft', 'Published', or 'Archived'."
                };
            }

            // Validate published products have at least one image
            if (request.Status == ProductStatus.Published && 
                (request.ImageUrls == null || request.ImageUrls.Count == 0))
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Published products must have at least one image."
                };
            }

            // Create product entity
            var product = new Product
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                SKU = request.SKU,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Weight = request.Weight,
                Length = request.Length,
                Width = request.Width,
                Height = request.Height,
                ImageUrls = request.ImageUrls != null ? JsonSerializer.Serialize(request.ImageUrls) : "[]",
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductId} for store {StoreId}", product.Id, storeId);

            return new ProductResponse
            {
                Success = true,
                Message = "Product created successfully",
                Product = await MapToDtoAsync(product)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product for store {StoreId}", storeId);
            return new ProductResponse
            {
                Success = false,
                Message = "An error occurred while creating the product. Please try again."
            };
        }
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid productId, Guid storeId, UpdateProductRequest request)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId);

            if (product == null)
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Product not found or you do not have permission to update it."
                };
            }

            // Validate category exists and is active
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null || !category.IsActive)
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Invalid or inactive category."
                };
            }

            // Validate status
            if (request.Status != ProductStatus.Draft && 
                request.Status != ProductStatus.Published && 
                request.Status != ProductStatus.Archived)
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Invalid product status. Must be 'Draft', 'Published', or 'Archived'."
                };
            }

            // Validate published products have at least one image
            if (request.Status == ProductStatus.Published && 
                (request.ImageUrls == null || request.ImageUrls.Count == 0))
            {
                return new ProductResponse
                {
                    Success = false,
                    Message = "Published products must have at least one image."
                };
            }

            // Update product properties
            product.Title = request.Title;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.Weight = request.Weight;
            product.Length = request.Length;
            product.Width = request.Width;
            product.Height = request.Height;
            product.ImageUrls = request.ImageUrls != null ? JsonSerializer.Serialize(request.ImageUrls) : "[]";
            product.Status = request.Status;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductId}", productId);

            return new ProductResponse
            {
                Success = true,
                Message = "Product updated successfully",
                Product = await MapToDtoAsync(product)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return new ProductResponse
            {
                Success = false,
                Message = "An error occurred while updating the product. Please try again."
            };
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId);

        return product != null ? await MapToDtoAsync(product) : null;
    }

    public async Task<List<ProductDto>> GetProductsByStoreIdAsync(Guid storeId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StoreId == storeId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var productDtos = new List<ProductDto>();
        foreach (var product in products)
        {
            productDtos.Add(await MapToDtoAsync(product));
        }

        return productDtos;
    }

    public async Task<List<PublicProductDto>> GetPublishedProductsByStoreIdAsync(Guid storeId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StoreId == storeId && p.Status == ProductStatus.Published && p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // TODO: Include store name - requires cross-module query or service call
        return products.Select(p => new PublicProductDto
        {
            Id = p.Id,
            StoreId = p.StoreId,
            StoreName = "", // TODO: Fetch from Store service
            Title = p.Title,
            Description = p.Description,
            CategoryName = p.Category?.Name,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ImageUrls = DeserializeImageUrls(p.ImageUrls),
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteProductAsync(Guid productId, Guid storeId)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId);

            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted: {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> IsSKUAvailableAsync(Guid storeId, string sku, Guid? excludeProductId = null)
    {
        var query = _context.Products.Where(p => p.StoreId == storeId && p.SKU == sku);
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return !await query.AnyAsync();
    }

    private async Task<ProductDto> MapToDtoAsync(Product product)
    {
        // Load category if not already loaded
        if (product.Category == null)
        {
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        }

        return new ProductDto
        {
            Id = product.Id,
            StoreId = product.StoreId,
            SKU = product.SKU,
            Title = product.Title,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            ImageUrls = DeserializeImageUrls(product.ImageUrls),
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static List<string> DeserializeImageUrls(string imageUrls)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(imageUrls) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

/// <summary>
/// Service for managing categories.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ProductCatalogDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ProductCatalogDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Category created: {CategoryId}", category.Id);

        return MapToDto(category);
    }

    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        return category != null ? MapToDto(category) : null;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}
