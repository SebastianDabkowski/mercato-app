using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for audit log viewing (client-side).
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ChangesJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PerformedAt { get; set; }
}

public class PaginatedAuditLogsResponse
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Service for viewing audit logs.
/// </summary>
public interface IAuditLogService
{
    Task<PaginatedAuditLogsResponse?> SearchAuditLogsAsync(
        string? adminUserId,
        string? entityType,
        string? entityId,
        string? action,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
    
    Task<List<AuditLogDto>?> GetEntityAuditLogsAsync(string entityType, string entityId);
}

public class AuditLogService : IAuditLogService
{
    private readonly HttpClient _httpClient;

    public AuditLogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedAuditLogsResponse?> SearchAuditLogsAsync(
        string? adminUserId,
        string? entityType,
        string? entityId,
        string? action,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize)
    {
        var query = $"?PageNumber={pageNumber}&PageSize={pageSize}";
        if (!string.IsNullOrEmpty(adminUserId))
            query += $"&AdminUserId={Uri.EscapeDataString(adminUserId)}";
        if (!string.IsNullOrEmpty(entityType))
            query += $"&EntityType={Uri.EscapeDataString(entityType)}";
        if (!string.IsNullOrEmpty(entityId))
            query += $"&EntityId={Uri.EscapeDataString(entityId)}";
        if (!string.IsNullOrEmpty(action))
            query += $"&Action={Uri.EscapeDataString(action)}";
        if (fromDate.HasValue)
            query += $"&FromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ss}";
        if (toDate.HasValue)
            query += $"&ToDate={toDate.Value:yyyy-MM-ddTHH:mm:ss}";

        return await _httpClient.GetFromJsonAsync<PaginatedAuditLogsResponse>($"/api/admin/audit-logs{query}");
    }

    public async Task<List<AuditLogDto>?> GetEntityAuditLogsAsync(string entityType, string entityId)
    {
        return await _httpClient.GetFromJsonAsync<List<AuditLogDto>>($"/api/admin/audit-logs/entity/{entityType}/{entityId}");
    }
}
