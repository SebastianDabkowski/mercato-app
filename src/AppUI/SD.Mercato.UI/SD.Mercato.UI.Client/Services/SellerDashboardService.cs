using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// Seller dashboard metrics DTO.
/// </summary>
public class SellerDashboardMetrics
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public List<BestSellingProduct> BestSellingProducts { get; set; } = new();
}

/// <summary>
/// Best-selling product DTO.
/// </summary>
public class BestSellingProduct
{
    public Guid ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Service interface for seller dashboard.
/// </summary>
public interface ISellerDashboardService
{
    Task<SellerDashboardMetrics?> GetDashboardMetricsAsync(Guid storeId, DateTime startDate, DateTime endDate);
}

/// <summary>
/// Service implementation for seller dashboard.
/// </summary>
public class SellerDashboardService : ISellerDashboardService
{
    private readonly HttpClient _http;
    private readonly ILogger<SellerDashboardService> _logger;

    public SellerDashboardService(HttpClient http, ILogger<SellerDashboardService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SellerDashboardMetrics?> GetDashboardMetricsAsync(Guid storeId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var startDateStr = startDate.ToString("yyyy-MM-dd");
            var endDateStr = endDate.ToString("yyyy-MM-dd");
            
            var response = await _http.GetAsync(
                $"api/seller/dashboard/metrics?storeId={storeId}&startDate={startDateStr}&endDate={endDateStr}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SellerDashboardMetrics>();
            }

            _logger.LogWarning(
                "Failed to fetch seller dashboard metrics for store {StoreId}: {StatusCode}", 
                storeId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching seller dashboard metrics for store {StoreId}", storeId);
            return null;
        }
    }
}
