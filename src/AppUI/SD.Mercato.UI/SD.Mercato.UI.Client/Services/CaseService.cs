using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for case management operations (client-side).
/// </summary>
public class CreateReturnRequestDto
{
    public Guid SubOrderId { get; set; }
    public Guid? SubOrderItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateComplaintRequestDto
{
    public Guid SubOrderId { get; set; }
    public Guid? SubOrderItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateCaseStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
}

public class AddCaseMessageRequestDto
{
    public string Message { get; set; } = string.Empty;
}

public class CaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SubOrderId { get; set; }
    public string SubOrderNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public Guid? SubOrderItemId { get; set; }
    public string? ProductTitle { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public List<CaseMessageDto> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CaseMessageDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCaseResponseDto
{
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CaseListResponseDto
{
    public List<CaseDto> Cases { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Service interface for case management operations.
/// </summary>
public interface ICaseService
{
    Task<CreateCaseResponseDto?> CreateReturnRequestAsync(CreateReturnRequestDto request);
    Task<CreateCaseResponseDto?> CreateComplaintAsync(CreateComplaintRequestDto request);
    Task<CaseListResponseDto?> GetBuyerCasesAsync(string? status = null, string? caseType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20);
    Task<CaseListResponseDto?> GetSellerCasesAsync(Guid storeId, string? status = null, string? caseType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20);
    Task<CaseDto?> GetCaseByIdAsync(Guid id);
    Task<bool> UpdateCaseStatusAsync(Guid id, UpdateCaseStatusRequestDto request);
    Task<bool> AddCaseMessageAsync(Guid id, AddCaseMessageRequestDto request);
}

/// <summary>
/// Service implementation for case management operations.
/// </summary>
public class CaseService : ICaseService
{
    private readonly HttpClient _httpClient;

    public CaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateCaseResponseDto?> CreateReturnRequestAsync(CreateReturnRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/cases/return", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateCaseResponseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating return request: {ex.Message}");
            return null;
        }
    }

    public async Task<CreateCaseResponseDto?> CreateComplaintAsync(CreateComplaintRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/cases/complaint", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateCaseResponseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating complaint: {ex.Message}");
            return null;
        }
    }

    public async Task<CaseListResponseDto?> GetBuyerCasesAsync(string? status = null, string? caseType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(caseType)) queryParams.Add($"caseType={Uri.EscapeDataString(caseType)}");
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"api/cases/buyer?{string.Join("&", queryParams)}";
            return await _httpClient.GetFromJsonAsync<CaseListResponseDto>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting buyer cases: {ex.Message}");
            return null;
        }
    }

    public async Task<CaseListResponseDto?> GetSellerCasesAsync(Guid storeId, string? status = null, string? caseType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(caseType)) queryParams.Add($"caseType={Uri.EscapeDataString(caseType)}");
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"api/cases/seller/{storeId}?{string.Join("&", queryParams)}";
            return await _httpClient.GetFromJsonAsync<CaseListResponseDto>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting seller cases: {ex.Message}");
            return null;
        }
    }

    public async Task<CaseDto?> GetCaseByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CaseDto>($"api/cases/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting case: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCaseStatusAsync(Guid id, UpdateCaseStatusRequestDto request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/cases/{id}/status", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating case status: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddCaseMessageAsync(Guid id, AddCaseMessageRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/cases/{id}/messages", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding case message: {ex.Message}");
            return false;
        }
    }
}
