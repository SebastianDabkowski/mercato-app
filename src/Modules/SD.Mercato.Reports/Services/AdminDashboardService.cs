using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.History.Data;
using SD.Mercato.History.Models;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.Models;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.SellerPanel.Data;
using SD.Mercato.SellerPanel.Models;
using SD.Mercato.Users.Data;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Implementation of admin dashboard service.
/// Aggregates metrics from various modules for the admin dashboard.
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly UsersDbContext _usersContext;
    private readonly SellerPanelDbContext _sellerContext;
    private readonly ProductCatalogDbContext _catalogContext;
    private readonly HistoryDbContext _historyContext;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        UsersDbContext usersContext,
        SellerPanelDbContext sellerContext,
        ProductCatalogDbContext catalogContext,
        HistoryDbContext historyContext,
        ILogger<AdminDashboardService> logger)
    {
        _usersContext = usersContext;
        _sellerContext = sellerContext;
        _catalogContext = catalogContext;
        _historyContext = historyContext;
        _logger = logger;
    }

    public async Task<AdminDashboardMetrics> GetDashboardMetricsAsync()
    {
        _logger.LogInformation("Generating admin dashboard metrics");

        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrowStart = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Execute queries in parallel for better performance
            var gmvTodayTask = CalculateGmvAsync(today, tomorrowStart);
            var gmvThisMonthTask = CalculateGmvAsync(monthStart, tomorrowStart);
            var ordersTodayTask = CountOrdersAsync(today, tomorrowStart);
            var ordersThisMonthTask = CountOrdersAsync(monthStart, tomorrowStart);
            var activeSellersTask = CountActiveSellersAsync();
            var activeListingsTask = CountActiveListingsAsync();
            var newBuyersTodayTask = CountNewUsersInRoleAsync("Buyer", today, tomorrowStart);
            var newBuyersThisMonthTask = CountNewUsersInRoleAsync("Buyer", monthStart, tomorrowStart);
            var newSellersTodayTask = CountNewUsersInRoleAsync("Seller", today, tomorrowStart);
            var newSellersThisMonthTask = CountNewUsersInRoleAsync("Seller", monthStart, tomorrowStart);

            await Task.WhenAll(
                gmvTodayTask,
                gmvThisMonthTask,
                ordersTodayTask,
                ordersThisMonthTask,
                activeSellersTask,
                activeListingsTask,
                newBuyersTodayTask,
                newBuyersThisMonthTask,
                newSellersTodayTask,
                newSellersThisMonthTask
            );

            var metrics = new AdminDashboardMetrics
            {
                GmvToday = await gmvTodayTask,
                GmvThisMonth = await gmvThisMonthTask,
                OrdersToday = await ordersTodayTask,
                OrdersThisMonth = await ordersThisMonthTask,
                ActiveSellers = await activeSellersTask,
                ActiveListings = await activeListingsTask,
                NewBuyersToday = await newBuyersTodayTask,
                NewBuyersThisMonth = await newBuyersThisMonthTask,
                NewSellersToday = await newSellersTodayTask,
                NewSellersThisMonth = await newSellersThisMonthTask
            };

            _logger.LogInformation(
                "Admin dashboard metrics generated: GMV Today={GmvToday}, Orders Today={OrdersToday}, Active Sellers={ActiveSellers}",
                metrics.GmvToday, metrics.OrdersToday, metrics.ActiveSellers);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin dashboard metrics");
            throw;
        }
    }

    /// <summary>
    /// Calculates total GMV (Gross Merchandise Value) for a date range.
    /// </summary>
    private async Task<decimal> CalculateGmvAsync(DateTime startDate, DateTime endDate)
    {
        // GMV = sum of all paid orders' total amounts
        var gmv = await _historyContext.Orders
            .Where(o => o.CreatedAt >= startDate 
                && o.CreatedAt < endDate
                && o.PaymentStatus == OrderPaymentStatus.Paid)
            .SumAsync(o => o.TotalAmount);

        return gmv;
    }

    /// <summary>
    /// Counts number of orders for a date range.
    /// </summary>
    private async Task<int> CountOrdersAsync(DateTime startDate, DateTime endDate)
    {
        var count = await _historyContext.Orders
            .Where(o => o.CreatedAt >= startDate 
                && o.CreatedAt < endDate
                && o.PaymentStatus == OrderPaymentStatus.Paid)
            .CountAsync();

        return count;
    }

    /// <summary>
    /// Counts number of active sellers (stores with Active status).
    /// </summary>
    private async Task<int> CountActiveSellersAsync()
    {
        // Count stores that are active (IsActive = true)
        var count = await _sellerContext.Stores
            .Where(s => s.IsActive)
            .CountAsync();

        return count;
    }

    /// <summary>
    /// Counts number of active product listings (Published products).
    /// </summary>
    private async Task<int> CountActiveListingsAsync()
    {
        var count = await _catalogContext.Products
            .Where(p => p.Status == ProductStatus.Published)
            .CountAsync();

        return count;
    }

    /// <summary>
    /// Counts new user registrations in a specific role for a date range.
    /// Uses efficient JOIN to avoid N+1 query pattern.
    /// </summary>
    private async Task<int> CountNewUsersInRoleAsync(string roleName, DateTime startDate, DateTime endDate)
    {
        // Get role ID for the specified role
        var role = await _usersContext.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null)
        {
            _logger.LogWarning("Role {RoleName} not found", roleName);
            return 0;
        }

        // Count users created in the date range who have this role (using JOIN for efficiency)
        var count = await _usersContext.Users
            .Join(_usersContext.UserRoles,
                u => u.Id,
                ur => ur.UserId,
                (u, ur) => new { User = u, UserRole = ur })
            .Where(x => x.UserRole.RoleId == role.Id
                && x.User.CreatedAt >= startDate
                && x.User.CreatedAt < endDate)
            .CountAsync();

        return count;
    }
}
