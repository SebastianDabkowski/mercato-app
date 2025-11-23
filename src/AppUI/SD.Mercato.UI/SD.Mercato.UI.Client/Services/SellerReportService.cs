using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTO for seller financial summary.
/// </summary>
public class SellerFinancialSummary
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalGMV { get; set; }
    public decimal TotalProductValue { get; set; }
    public decimal TotalShippingFees { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalProcessingFees { get; set; }
    public decimal NetAmountDue { get; set; }
    public int OrderCount { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// DTO for commission line item.
/// </summary>
public class CommissionLineItem
{
    public Guid SubOrderId { get; set; }
    public string SubOrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
}

/// <summary>
/// DTO for seller invoice.
/// </summary>
public class SellerInvoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalGMV { get; set; }
    public decimal TotalProductValue { get; set; }
    public decimal TotalShippingFees { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalProcessingFees { get; set; }
    public decimal NetAmountDue { get; set; }
    public int OrderCount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Response for invoice generation.
/// </summary>
public class GenerateInvoiceResponse
{
    public bool Success { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Service interface for seller financial reports.
/// </summary>
public interface ISellerReportService
{
    Task<SellerFinancialSummary?> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate);
    Task<List<CommissionLineItem>> GetCommissionBreakdownAsync(DateTime startDate, DateTime endDate);
    Task<GenerateInvoiceResponse> GenerateInvoiceAsync(DateTime startDate, DateTime endDate);
    Task<List<SellerInvoice>> GetInvoicesAsync();
    Task<string> DownloadInvoiceAsync(Guid invoiceId);
}

/// <summary>
/// Service for seller financial reporting operations.
/// </summary>
public class SellerReportService : ISellerReportService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SellerReportService> _logger;
    private Guid? _cachedStoreId;

    public SellerReportService(HttpClient httpClient, ILogger<SellerReportService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SellerFinancialSummary?> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var storeId = await GetStoreIdAsync();
            if (!storeId.HasValue)
            {
                _logger.LogWarning("No store found for current seller");
                return null;
            }

            var response = await _httpClient.GetFromJsonAsync<SellerFinancialSummary>(
                $"api/sellerreports/summary?storeId={storeId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching financial summary");
            throw;
        }
    }

    public async Task<List<CommissionLineItem>> GetCommissionBreakdownAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var storeId = await GetStoreIdAsync();
            if (!storeId.HasValue)
            {
                _logger.LogWarning("No store found for current seller");
                return new List<CommissionLineItem>();
            }

            var response = await _httpClient.GetFromJsonAsync<List<CommissionLineItem>>(
                $"api/sellerreports/commission-breakdown?storeId={storeId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            return response ?? new List<CommissionLineItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commission breakdown");
            throw;
        }
    }

    public async Task<GenerateInvoiceResponse> GenerateInvoiceAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var storeId = await GetStoreIdAsync();
            if (!storeId.HasValue)
            {
                return new GenerateInvoiceResponse
                {
                    Success = false,
                    ErrorMessage = "No store found for current seller"
                };
            }

            var request = new
            {
                StoreId = storeId.Value,
                PeriodStartDate = startDate,
                PeriodEndDate = endDate
            };

            var response = await _httpClient.PostAsJsonAsync("api/sellerreports/generate-invoice", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GenerateInvoiceResponse>();
            return result ?? new GenerateInvoiceResponse { Success = false, ErrorMessage = "Invalid response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            return new GenerateInvoiceResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<SellerInvoice>> GetInvoicesAsync()
    {
        try
        {
            var storeId = await GetStoreIdAsync();
            if (!storeId.HasValue)
            {
                _logger.LogWarning("No store found for current seller");
                return new List<SellerInvoice>();
            }

            var response = await _httpClient.GetFromJsonAsync<List<SellerInvoice>>(
                $"api/sellerreports/invoices?storeId={storeId}");

            return response ?? new List<SellerInvoice>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching invoices");
            throw;
        }
    }

    public async Task<string> DownloadInvoiceAsync(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/sellerreports/invoices/{invoiceId}/download");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading invoice");
            throw;
        }
    }

    private async Task<Guid?> GetStoreIdAsync()
    {
        if (_cachedStoreId.HasValue)
        {
            return _cachedStoreId;
        }

        try
        {
            // TODO: This should fetch the seller's store ID from the API
            // For now, we'll need to add an endpoint to get the current seller's store
            // GET /api/seller/my-store
            var response = await _httpClient.GetFromJsonAsync<MyStoreResponse>("api/stores/my-store");
            _cachedStoreId = response?.StoreId;
            return _cachedStoreId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching store ID");
            return null;
        }
    }

    private class MyStoreResponse
    {
        public Guid StoreId { get; set; }
    }
}
