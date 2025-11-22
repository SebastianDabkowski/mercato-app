using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for admin category management (client-side).
/// </summary>
public class AdminCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? DefaultCommissionRate { get; set; }
    public int ProductCount { get; set; }
}

public class AdminCreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public decimal? DefaultCommissionRate { get; set; }
}

public class AdminUpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public decimal? DefaultCommissionRate { get; set; }
    public bool? IsActive { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Service for admin category management operations.
/// </summary>
public interface IAdminCategoryService
{
    Task<List<AdminCategoryDto>?> GetAllCategoriesAsync();
    Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid categoryId);
    Task<AdminCategoryDto?> CreateCategoryAsync(AdminCreateCategoryRequest request);
    Task<AdminCategoryDto?> UpdateCategoryAsync(Guid categoryId, AdminUpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid categoryId);
}

public class AdminCategoryService : IAdminCategoryService
{
    private readonly HttpClient _httpClient;

    public AdminCategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AdminCategoryDto>?> GetAllCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AdminCategoryDto>>("/api/admin/categories");
    }

    public async Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        return await _httpClient.GetFromJsonAsync<AdminCategoryDto>($"/api/admin/categories/{categoryId}");
    }

    public async Task<AdminCategoryDto?> CreateCategoryAsync(AdminCreateCategoryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/admin/categories", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AdminCategoryDto>();
        }
        return null;
    }

    public async Task<AdminCategoryDto?> UpdateCategoryAsync(Guid categoryId, AdminUpdateCategoryRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/admin/categories/{categoryId}", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AdminCategoryDto>();
        }
        return null;
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        var response = await _httpClient.DeleteAsync($"/api/admin/categories/{categoryId}");
        return response.IsSuccessStatusCode;
    }
}
