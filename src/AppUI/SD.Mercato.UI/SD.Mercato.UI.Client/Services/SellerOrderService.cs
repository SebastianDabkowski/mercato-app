using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// Request model for updating SubOrder status.
/// </summary>
public class UpdateSubOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Response model for SubOrder list with pagination.
/// </summary>
public class SubOrderListResponse
{
    public List<SubOrderDto> SubOrders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>
/// Response model for Order list with pagination.
/// </summary>
public class OrderListResponse
{
    public List<OrderDto> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>
/// Service interface for seller order operations.
/// </summary>
public interface ISellerOrderService
{
    Task<SubOrderListResponse?> GetSubOrdersAsync(
        string? status = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20);
    
    Task<SubOrderDto?> GetSubOrderByIdAsync(Guid subOrderId);
    
    Task<SubOrderDto?> UpdateSubOrderStatusAsync(Guid subOrderId, UpdateSubOrderStatusRequest request);
}

/// <summary>
/// Service for seller order operations.
/// </summary>
public class SellerOrderService : ISellerOrderService
{
    private readonly HttpClient _httpClient;

    public SellerOrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SubOrderListResponse?> GetSubOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");
            
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");
            
            var queryString = string.Join("&", queryParams);
            var url = $"/api/seller/orders?{queryString}";
            
            return await _httpClient.GetFromJsonAsync<SubOrderListResponse>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting seller orders: {ex.Message}");
            return null;
        }
    }

    public async Task<SubOrderDto?> GetSubOrderByIdAsync(Guid subOrderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SubOrderDto>($"/api/seller/orders/{subOrderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting seller order: {ex.Message}");
            return null;
        }
    }

    public async Task<SubOrderDto?> UpdateSubOrderStatusAsync(Guid subOrderId, UpdateSubOrderStatusRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/seller/orders/{subOrderId}/status", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SubOrderDto>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Update status failed: {response.StatusCode} - {errorContent}");
            
            // Try to extract error message from response
            try
            {
                var errorObj = System.Text.Json.JsonDocument.Parse(errorContent);
                if (errorObj.RootElement.TryGetProperty("message", out var messageElement))
                {
                    throw new HttpRequestException(messageElement.GetString());
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // If JSON parsing fails, throw with raw error content if it's short enough
                if (errorContent.Length < 200)
                {
                    throw new HttpRequestException(errorContent);
                }
            }
            
            throw new HttpRequestException($"Update failed with status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating order status: {ex.Message}");
            throw;
        }
    }
}
