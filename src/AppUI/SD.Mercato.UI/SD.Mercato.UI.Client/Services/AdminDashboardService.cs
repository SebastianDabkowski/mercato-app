using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// Admin dashboard metrics DTO.
/// </summary>
public class AdminDashboardMetrics
{
    public decimal GmvToday { get; set; }
    public decimal GmvThisMonth { get; set; }
    public int OrdersToday { get; set; }
    public int OrdersThisMonth { get; set; }
    public int ActiveSellers { get; set; }
    public int ActiveListings { get; set; }
    public int NewBuyersToday { get; set; }
    public int NewBuyersThisMonth { get; set; }
    public int NewSellersToday { get; set; }
    public int NewSellersThisMonth { get; set; }
}

/// <summary>
/// Service interface for admin dashboard.
/// </summary>
public interface IAdminDashboardService
{
    Task<AdminDashboardMetrics?> GetDashboardMetricsAsync();
}

/// <summary>
/// Service implementation for admin dashboard.
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly HttpClient _http;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(HttpClient http, ILogger<AdminDashboardService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AdminDashboardMetrics?> GetDashboardMetricsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/admin/dashboard/metrics");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdminDashboardMetrics>();
            }

            _logger.LogWarning("Failed to fetch admin dashboard metrics: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin dashboard metrics");
            return null;
        }
    }
}
