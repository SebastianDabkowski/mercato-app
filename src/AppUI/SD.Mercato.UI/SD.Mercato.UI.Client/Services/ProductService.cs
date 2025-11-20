using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for products and categories (client-side).
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
    public string Currency { get; set; } = "USD";
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

public class PublicProductDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int StockQuantity { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateProductRequest
{
    public string SKU { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int StockQuantity { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string Status { get; set; } = "Draft";
}

public class UpdateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int StockQuantity { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string Status { get; set; } = "Draft";
}

public class ProductResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ProductDto? Product { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for searching and filtering products.
/// </summary>
public class ProductSearchRequest
{
    public string? SearchQuery { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? StoreId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

/// <summary>
/// Paginated response for product search results.
/// </summary>
public class PaginatedProductsResponse
{
    public List<PublicProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Lightweight store DTO for listing purposes.
/// </summary>
public class StoreListItemDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}

/// <summary>
/// Interface for product service.
/// </summary>
public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateProductAsync(Guid productId, UpdateProductRequest request);
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
    Task<List<ProductDto>> GetMyProductsAsync();
    Task<List<PublicProductDto>> GetCatalogAsync();
    Task<PaginatedProductsResponse> SearchProductsAsync(ProductSearchRequest request);
    Task<bool> DeleteProductAsync(Guid productId);
}

/// <summary>
/// Interface for category service.
/// </summary>
public interface ICategoryService
{
    Task<CategoryDto?> CreateCategoryAsync(CreateCategoryRequest request);
    Task<List<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);
}

/// <summary>
/// Product service for Blazor WebAssembly client.
/// </summary>
public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", request);
            var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
            return result ?? new ProductResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            return new ProductResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid productId, UpdateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{productId}", request);
            var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
            return result ?? new ProductResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            return new ProductResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{productId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ProductDto>> GetMyProductsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/products/my-products");
            return result ?? new List<ProductDto>();
        }
        catch
        {
            return new List<ProductDto>();
        }
    }

    public async Task<List<PublicProductDto>> GetCatalogAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<PublicProductDto>>("api/products/catalog");
            return result ?? new List<PublicProductDto>();
        }
        catch
        {
            return new List<PublicProductDto>();
        }
    }

    public async Task<PaginatedProductsResponse> SearchProductsAsync(ProductSearchRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products/search", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaginatedProductsResponse>();
                return result ?? new PaginatedProductsResponse();
            }
            return new PaginatedProductsResponse();
        }
        catch (HttpRequestException)
        {
            return new PaginatedProductsResponse();
        }
        catch (TaskCanceledException)
        {
            return new PaginatedProductsResponse();
        }
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/products/{productId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Category service for Blazor WebAssembly client.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;

    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CategoryDto?> CreateCategoryAsync(CreateCategoryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/categories", request);
            return await response.Content.ReadFromJsonAsync<CategoryDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/categories");
            return result ?? new List<CategoryDto>();
        }
        catch
        {
            return new List<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CategoryDto>($"api/categories/{categoryId}");
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Interface for store service.
/// </summary>
public interface IStoreService
{
    Task<List<StoreListItemDto>> GetActiveStoresAsync();
}

/// <summary>
/// Store service for Blazor WebAssembly client.
/// </summary>
public class StoreService : IStoreService
{
    private readonly HttpClient _httpClient;

    public StoreService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StoreListItemDto>> GetActiveStoresAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<StoreListItemDto>>("api/stores/active");
            return result ?? new List<StoreListItemDto>();
        }
        catch
        {
            return new List<StoreListItemDto>();
        }
    }
}
