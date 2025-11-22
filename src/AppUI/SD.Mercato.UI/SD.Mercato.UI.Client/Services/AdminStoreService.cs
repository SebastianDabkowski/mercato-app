using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for admin store management (client-side).
/// </summary>
public class AdminStoreListDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class AdminStoreDetailDto
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string StoreType { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public decimal CommissionRate { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProductCount { get; set; }
    public int TotalOrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public int CompletedOrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommissionEarned { get; set; }
}

public class PaginatedStoresResponse
{
    public List<AdminStoreListDto> Stores { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class UpdateStoreStatusRequest
{
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public string? Reason { get; set; }
}

public class UpdateStoreCommissionRequest
{
    public decimal CommissionRate { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Service for admin store management operations.
/// </summary>
public interface IAdminStoreService
{
    Task<PaginatedStoresResponse?> SearchStoresAsync(string? searchTerm, bool? isActive, bool? isVerified, int pageNumber, int pageSize);
    Task<AdminStoreDetailDto?> GetStoreDetailAsync(Guid storeId);
    Task<bool> UpdateStoreStatusAsync(Guid storeId, UpdateStoreStatusRequest request);
    Task<bool> UpdateStoreCommissionAsync(Guid storeId, UpdateStoreCommissionRequest request);
}

public class AdminStoreService : IAdminStoreService
{
    private readonly HttpClient _httpClient;

    public AdminStoreService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedStoresResponse?> SearchStoresAsync(
        string? searchTerm,
        bool? isActive,
        bool? isVerified,
        int pageNumber,
        int pageSize)
    {
        var query = $"?PageNumber={pageNumber}&PageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm))
            query += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
        if (isActive.HasValue)
            query += $"&IsActive={isActive.Value}";
        if (isVerified.HasValue)
            query += $"&IsVerified={isVerified.Value}";

        return await _httpClient.GetFromJsonAsync<PaginatedStoresResponse>($"/api/admin/stores{query}");
    }

    public async Task<AdminStoreDetailDto?> GetStoreDetailAsync(Guid storeId)
    {
        return await _httpClient.GetFromJsonAsync<AdminStoreDetailDto>($"/api/admin/stores/{storeId}");
    }

    public async Task<bool> UpdateStoreStatusAsync(Guid storeId, UpdateStoreStatusRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/admin/stores/{storeId}/status", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStoreCommissionAsync(Guid storeId, UpdateStoreCommissionRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/admin/stores/{storeId}/commission", request);
        return response.IsSuccessStatusCode;
    }
}
