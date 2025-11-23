using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.History.Data;
using SD.Mercato.History.Models;
using SD.Mercato.Payments.Data;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.SellerPanel.Data;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Implementation of seller financial reporting service.
/// Queries payment and order data to generate seller reports.
/// </summary>
public class SellerReportService : ISellerReportService
{
    private readonly PaymentsDbContext _paymentsContext;
    private readonly HistoryDbContext _historyContext;
    private readonly SellerPanelDbContext _sellerContext;
    private readonly ILogger<SellerReportService> _logger;

    public SellerReportService(
        PaymentsDbContext paymentsContext,
        HistoryDbContext historyContext,
        SellerPanelDbContext sellerContext,
        ILogger<SellerReportService> logger)
    {
        _paymentsContext = paymentsContext;
        _historyContext = historyContext;
        _sellerContext = sellerContext;
        _logger = logger;
    }

    public async Task<SellerFinancialSummary?> GetFinancialSummaryAsync(SellerFinancialReportRequest request)
    {
        _logger.LogInformation("Generating financial summary for Store {StoreId} from {StartDate} to {EndDate}",
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

            // Query SubOrderPayments for the period
            // Filter by SubOrders that were delivered/completed during the period
            var subOrderPayments = await _paymentsContext.SubOrderPayments
                .Where(sp => sp.StoreId == request.StoreId
                    && sp.CreatedAt >= request.StartDate
                    && sp.CreatedAt <= request.EndDate
                    && sp.PayoutStatus != Payments.Models.SubOrderPayoutStatus.Pending) // Only count released/paid out orders
                .ToListAsync();

            // Calculate totals
            var totalGMV = subOrderPayments.Sum(sp => sp.SubOrderTotal);
            var totalProductValue = subOrderPayments.Sum(sp => sp.ProductTotal);
            var totalShippingFees = subOrderPayments.Sum(sp => sp.ShippingCost);
            var totalCommission = subOrderPayments.Sum(sp => sp.CommissionAmount);
            var totalProcessingFees = subOrderPayments.Sum(sp => sp.ProcessingFeeAllocated);
            var netAmountDue = subOrderPayments.Sum(sp => sp.SellerNetAmount);
            var orderCount = subOrderPayments.Count;

            var summary = new SellerFinancialSummary
            {
                StoreId = request.StoreId,
                StoreName = store.DisplayName,
                PeriodStartDate = request.StartDate,
                PeriodEndDate = request.EndDate,
                TotalGMV = totalGMV,
                TotalProductValue = totalProductValue,
                TotalShippingFees = totalShippingFees,
                TotalCommission = totalCommission,
                TotalProcessingFees = totalProcessingFees,
                NetAmountDue = netAmountDue,
                OrderCount = orderCount,
                Currency = "USD"
            };

            _logger.LogInformation("Financial summary generated: GMV={GMV}, Commission={Commission}, Net={Net}",
                totalGMV, totalCommission, netAmountDue);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial summary for Store {StoreId}", request.StoreId);
            throw;
        }
    }

    public async Task<List<CommissionLineItem>> GetCommissionBreakdownAsync(SellerFinancialReportRequest request)
    {
        _logger.LogInformation("Generating commission breakdown for Store {StoreId} from {StartDate} to {EndDate}",
            request.StoreId, request.StartDate, request.EndDate);

        try
        {
            // Get SubOrderPayments with related SubOrders
            var subOrderPayments = await _paymentsContext.SubOrderPayments
                .Where(sp => sp.StoreId == request.StoreId
                    && sp.CreatedAt >= request.StartDate
                    && sp.CreatedAt <= request.EndDate
                    && sp.PayoutStatus != Payments.Models.SubOrderPayoutStatus.Pending)
                .ToListAsync();

            var subOrderIds = subOrderPayments.Select(sp => sp.SubOrderId).ToList();

            // Get SubOrders with items
            var subOrders = await _historyContext.SubOrders
                .Include(so => so.Items)
                .Where(so => subOrderIds.Contains(so.Id))
                .ToListAsync();

            // Build commission line items
            var lineItems = new List<CommissionLineItem>();

            foreach (var subOrder in subOrders)
            {
                var payment = subOrderPayments.First(sp => sp.SubOrderId == subOrder.Id);

                foreach (var item in subOrder.Items)
                {
                    // Calculate commission for this line item
                    // Commission is applied proportionally based on item's share of total product value
                    var itemCommission = item.Subtotal * payment.CommissionRate;

                    lineItems.Add(new CommissionLineItem
                    {
                        SubOrderId = subOrder.Id,
                        SubOrderNumber = subOrder.SubOrderNumber,
                        OrderDate = subOrder.CreatedAt,
                        ProductSku = item.ProductSku,
                        ProductTitle = item.ProductTitle,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal,
                        CommissionRate = payment.CommissionRate,
                        CommissionAmount = Math.Round(itemCommission, 2)
                    });
                }
            }

            _logger.LogInformation("Commission breakdown generated: {Count} line items", lineItems.Count);

            return lineItems.OrderBy(li => li.OrderDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating commission breakdown for Store {StoreId}", request.StoreId);
            throw;
        }
    }
}
