using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.History.Data;
using SD.Mercato.History.DTOs;
using System.Globalization;
using System.Text;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service for exporting orders in CSV format.
/// </summary>
public class OrderCsvService : IOrderCsvService
{
    private readonly HistoryDbContext _context;
    private readonly ILogger<OrderCsvService> _logger;

    // Commission and fee constants (matching platform business rules)
    private const decimal PLATFORM_COMMISSION_RATE = 0.15m; // 15%
    private const decimal PROCESSING_FEE_PERCENTAGE = 0.029m; // 2.9%
    private const decimal PROCESSING_FEE_FIXED = 0.30m; // $0.30

    public OrderCsvService(
        HistoryDbContext context,
        ILogger<OrderCsvService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderCsvExportResult> ExportOrdersAsync(Guid storeId, ExportOrdersCsvRequest request)
    {
        try
        {
            var query = _context.SubOrders
                .Include(so => so.Order)
                .Include(so => so.Items)
                .Where(so => so.StoreId == storeId);

            // Apply date filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(so => so.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                // Include the entire end date by adding one day (start of next day is exclusive upper bound)
                var endOfDay = request.ToDate.Value.Date.AddDays(1);
                query = query.Where(so => so.CreatedAt < endOfDay);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(so => so.Status == request.Status);
            }

            var subOrders = await query
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();

            if (subOrders.Count == 0)
            {
                return new OrderCsvExportResult
                {
                    Success = true,
                    RecordCount = 0,
                    FileContent = Array.Empty<byte>(),
                    FileName = GenerateFileName(request),
                    ErrorMessage = null
                };
            }

            // Generate CSV rows
            var csvRows = new List<OrderCsvRow>();

            foreach (var subOrder in subOrders)
            {
                // Calculate commission and fees for this suborder
                var productsTotal = subOrder.ProductsTotal;
                var shippingCost = subOrder.ShippingCost;
                var totalAmount = subOrder.TotalAmount;

                // Commission is on products only (not shipping)
                var platformCommission = productsTotal * PLATFORM_COMMISSION_RATE;

                // Processing fee is on total (products + shipping)
                var processingFee = (totalAmount * PROCESSING_FEE_PERCENTAGE) + PROCESSING_FEE_FIXED;

                // Net amount seller receives
                var netAmount = totalAmount - platformCommission - processingFee;

                // Create one row per item
                for (int i = 0; i < subOrder.Items.Count; i++)
                {
                    var item = subOrder.Items[i];
                    var isFirstRow = i == 0;

                    var row = new OrderCsvRow
                    {
                        OrderNumber = subOrder.Order?.OrderNumber ?? string.Empty,
                        SubOrderNumber = subOrder.SubOrderNumber,
                        OrderDate = subOrder.CreatedAt,
                        Status = subOrder.Status,
                        BuyerEmail = subOrder.Order?.BuyerEmail ?? string.Empty,
                        BuyerPhone = subOrder.Order?.BuyerPhone ?? string.Empty,
                        RecipientName = subOrder.Order?.DeliveryRecipientName ?? string.Empty,
                        DeliveryAddress = FormatAddress(subOrder.Order),
                        ProductSKU = item.ProductSku,
                        ProductTitle = item.ProductTitle,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineSubtotal = item.Subtotal,
                        // Show totals and fees on first item row only to avoid double-counting
                        ShippingCost = isFirstRow ? shippingCost : 0,
                        SubOrderTotal = isFirstRow ? totalAmount : 0,
                        PlatformCommission = isFirstRow ? platformCommission : 0,
                        ProcessingFee = isFirstRow ? processingFee : 0,
                        NetAmount = isFirstRow ? netAmount : 0,
                        ShippingMethod = subOrder.ShippingMethod,
                        TrackingNumber = subOrder.TrackingNumber,
                        ShippedDate = subOrder.ShippedAt,
                        DeliveredDate = subOrder.DeliveredAt
                    };

                    csvRows.Add(row);
                }
            }

            // Generate CSV file
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecords(csvRows);
            await writer.FlushAsync();

            var fileContent = memoryStream.ToArray();

            _logger.LogInformation(
                "Exported {RowCount} order line items for store {StoreId} ({SubOrderCount} sub-orders)",
                csvRows.Count, storeId, subOrders.Count);

            return new OrderCsvExportResult
            {
                Success = true,
                RecordCount = csvRows.Count,
                FileContent = fileContent,
                FileName = GenerateFileName(request)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orders for store {StoreId}", storeId);
            return new OrderCsvExportResult
            {
                Success = false,
                RecordCount = 0,
                ErrorMessage = $"Export failed: {ex.Message}"
            };
        }
    }

    private string FormatAddress(Models.Order? order)
    {
        if (order == null)
        {
            return string.Empty;
        }

        var parts = new List<string>
        {
            order.DeliveryAddressLine1
        };

        if (!string.IsNullOrWhiteSpace(order.DeliveryAddressLine2))
        {
            parts.Add(order.DeliveryAddressLine2);
        }

        parts.Add($"{order.DeliveryCity}, {order.DeliveryState} {order.DeliveryPostalCode}");
        parts.Add(order.DeliveryCountry);

        return string.Join(", ", parts);
    }

    private string GenerateFileName(ExportOrdersCsvRequest request)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var datePart = string.Empty;

        if (request.FromDate.HasValue && request.ToDate.HasValue)
        {
            datePart = $"_{request.FromDate.Value:yyyyMMdd}-{request.ToDate.Value:yyyyMMdd}";
        }
        else if (request.FromDate.HasValue)
        {
            datePart = $"_from_{request.FromDate.Value:yyyyMMdd}";
        }
        else if (request.ToDate.HasValue)
        {
            datePart = $"_until_{request.ToDate.Value:yyyyMMdd}";
        }

        return $"orders_export{datePart}_{timestamp}.csv";
    }
}
