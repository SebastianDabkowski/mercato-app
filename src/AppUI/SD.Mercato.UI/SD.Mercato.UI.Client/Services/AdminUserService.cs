using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for admin user management (client-side).
/// </summary>
public class AdminUserListDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminUserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalProviderId { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
}

public class PaginatedUsersResponse
{
    public List<AdminUserListDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}

public class AdminPasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}

public class AdminUserInviteRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Message { get; set; }
}

/// <summary>
/// Service for admin user management operations.
/// </summary>
public interface IAdminUserService
{
    Task<PaginatedUsersResponse?> SearchUsersAsync(string? searchTerm, string? role, bool? isActive, bool? isEmailVerified, int pageNumber, int pageSize);
    Task<AdminUserDetailDto?> GetUserDetailAsync(string userId);
    Task<bool> UpdateUserStatusAsync(string userId, UpdateUserStatusRequest request);
    Task<bool> SendPasswordResetAsync(string email);
    Task<bool> SendUserInviteAsync(AdminUserInviteRequest request);
}

public class AdminUserService : IAdminUserService
{
    private readonly HttpClient _httpClient;

    public AdminUserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedUsersResponse?> SearchUsersAsync(
        string? searchTerm,
        string? role,
        bool? isActive,
        bool? isEmailVerified,
        int pageNumber,
        int pageSize)
    {
        var query = $"?PageNumber={pageNumber}&PageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm))
            query += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
        if (!string.IsNullOrEmpty(role))
            query += $"&Role={Uri.EscapeDataString(role)}";
        if (isActive.HasValue)
            query += $"&IsActive={isActive.Value}";
        if (isEmailVerified.HasValue)
            query += $"&IsEmailVerified={isEmailVerified.Value}";

        return await _httpClient.GetFromJsonAsync<PaginatedUsersResponse>($"/api/admin/users{query}");
    }

    public async Task<AdminUserDetailDto?> GetUserDetailAsync(string userId)
    {
        return await _httpClient.GetFromJsonAsync<AdminUserDetailDto>($"/api/admin/users/{userId}");
    }

    public async Task<bool> UpdateUserStatusAsync(string userId, UpdateUserStatusRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/admin/users/{userId}/status", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendPasswordResetAsync(string email)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/admin/users/password-reset", new AdminPasswordResetRequest { Email = email });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendUserInviteAsync(AdminUserInviteRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/admin/users/invite", request);
        return response.IsSuccessStatusCode;
    }
}
