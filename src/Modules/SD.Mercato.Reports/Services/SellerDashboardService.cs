using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.History.Data;
using SD.Mercato.History.Models;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.SellerPanel.Data;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Implementation of seller dashboard service.
/// Provides sales metrics and best-selling products for sellers.
/// </summary>
public class SellerDashboardService : ISellerDashboardService
{
    private readonly HistoryDbContext _historyContext;
    private readonly SellerPanelDbContext _sellerContext;
    private readonly ILogger<SellerDashboardService> _logger;

    public SellerDashboardService(
        HistoryDbContext historyContext,
        SellerPanelDbContext sellerContext,
        ILogger<SellerDashboardService> logger)
    {
        _historyContext = historyContext;
        _sellerContext = sellerContext;
        _logger = logger;
    }

    public async Task<SellerDashboardMetrics?> GetDashboardMetricsAsync(SellerDashboardRequest request)
    {
        _logger.LogInformation(
            "Generating seller dashboard metrics for Store {StoreId} from {StartDate} to {EndDate}",
            request.StoreId, request.StartDate, request.EndDate);

        try
        {
            // Get store info
            var store = await _sellerContext.Stores
                .FirstOrDefaultAsync(s => s.Id == request.StoreId);

            if (store == null)
            {
                _logger.LogWarning("Store {StoreId} not found", request.StoreId);
                return null;
            }

            // Get SubOrders for the period
            var subOrders = await _historyContext.SubOrders
                .Include(so => so.Items)
                .Where(so => so.StoreId == request.StoreId
                    && so.CreatedAt >= request.StartDate
                    && so.CreatedAt < request.EndDate
                    && (so.Status == SubOrderStatus.Delivered 
                        || so.Status == SubOrderStatus.Completed
                        || so.Status == SubOrderStatus.Shipped
                        || so.Status == SubOrderStatus.Processing))
                .ToListAsync();

            // Calculate total sales and order count
            var totalSales = subOrders.Sum(so => so.TotalAmount);
            var orderCount = subOrders.Count;

            // Calculate best-selling products
            var productSales = subOrders
                .SelectMany(so => so.Items)
                .GroupBy(item => new 
                { 
                    item.ProductId, 
                    item.ProductSku, 
                    item.ProductTitle, 
                    item.ProductImageUrl 
                })
                .Select(group => new BestSellingProduct
                {
                    ProductId = group.Key.ProductId,
                    ProductSku = group.Key.ProductSku,
                    ProductTitle = group.Key.ProductTitle,
                    ProductImageUrl = group.Key.ProductImageUrl,
                    QuantitySold = group.Sum(item => item.Quantity),
                    TotalRevenue = group.Sum(item => item.Subtotal)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10) // Top 10 best-selling products
                .ToList();

            var metrics = new SellerDashboardMetrics
            {
                StoreId = request.StoreId,
                StoreName = store.DisplayName,
                PeriodStartDate = request.StartDate,
                PeriodEndDate = request.EndDate,
                TotalSales = totalSales,
                OrderCount = orderCount,
                BestSellingProducts = productSales
            };

            _logger.LogInformation(
                "Seller dashboard metrics generated: Total Sales={TotalSales}, Orders={OrderCount}, Best Sellers={BestSellersCount}",
                totalSales, orderCount, productSales.Count);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating seller dashboard metrics for Store {StoreId}", request.StoreId);
            throw;
        }
    }
}
