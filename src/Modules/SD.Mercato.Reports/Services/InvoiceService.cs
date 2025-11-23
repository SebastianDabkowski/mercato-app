using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Reports.Data;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Models;
using System.Text;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Implementation of invoice generation and management service.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ReportsDbContext _context;
    private readonly ISellerReportService _reportService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        ReportsDbContext context,
        ISellerReportService reportService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _reportService = reportService;
        _logger = logger;
    }

    public async Task<GenerateInvoiceResponse> GenerateInvoiceAsync(GenerateInvoiceRequest request)
    {
        _logger.LogInformation("Generating invoice for Store {StoreId} for period {StartDate} to {EndDate}",
            request.StoreId, request.PeriodStartDate, request.PeriodEndDate);

        try
        {
            // Check if invoice already exists for this period
            var existingInvoice = await _context.SellerInvoices
                .FirstOrDefaultAsync(i => i.StoreId == request.StoreId
                    && i.PeriodStartDate == request.PeriodStartDate
                    && i.PeriodEndDate == request.PeriodEndDate);

            if (existingInvoice != null)
            {
                _logger.LogWarning("Invoice already exists for Store {StoreId} for this period: {InvoiceId}",
                    request.StoreId, existingInvoice.Id);

                return new GenerateInvoiceResponse
                {
                    Success = true,
                    InvoiceId = existingInvoice.Id,
                    InvoiceNumber = existingInvoice.InvoiceNumber
                };
            }

            // Get financial summary
            var summary = await _reportService.GetFinancialSummaryAsync(new SellerFinancialReportRequest
            {
                StoreId = request.StoreId,
                StartDate = request.PeriodStartDate,
                EndDate = request.PeriodEndDate
            });

            if (summary == null)
            {
                return new GenerateInvoiceResponse
                {
                    Success = false,
                    ErrorMessage = "Store not found or no data available for the period"
                };
            }

            // Generate invoice number
            var invoiceNumber = GenerateInvoiceNumber(request.StoreId, request.PeriodStartDate);

            // Generate HTML content
            var htmlContent = GenerateInvoiceHtml(summary, invoiceNumber);

            // Create invoice record
            var invoice = new SellerInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                StoreId = request.StoreId,
                StoreName = summary.StoreName,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                TotalGMV = summary.TotalGMV,
                TotalProductValue = summary.TotalProductValue,
                TotalShippingFees = summary.TotalShippingFees,
                TotalCommission = summary.TotalCommission,
                TotalProcessingFees = summary.TotalProcessingFees,
                NetAmountDue = summary.NetAmountDue,
                OrderCount = summary.OrderCount,
                Currency = summary.Currency,
                Status = InvoiceStatus.Generated,
                HtmlContent = htmlContent,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = request.GeneratedBy
            };

            _context.SellerInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Invoice generated: {InvoiceNumber} (ID: {InvoiceId})",
                invoiceNumber, invoice.Id);

            return new GenerateInvoiceResponse
            {
                Success = true,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoiceNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for Store {StoreId}", request.StoreId);
            return new GenerateInvoiceResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while generating the invoice"
            };
        }
    }

    public async Task<string?> GetInvoiceHtmlAsync(Guid invoiceId)
    {
        var invoice = await _context.SellerInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        return invoice?.HtmlContent;
    }

    public async Task<SellerInvoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        return await _context.SellerInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
    }

    public async Task<List<SellerInvoice>> GetStoreInvoicesAsync(Guid storeId)
    {
        return await _context.SellerInvoices
            .Where(i => i.StoreId == storeId)
            .OrderByDescending(i => i.PeriodEndDate)
            .ToListAsync();
    }

    private string GenerateInvoiceNumber(Guid storeId, DateTime periodStart)
    {
        // Format: INV-YYYY-MM-STOREID_SHORT
        var storeIdShort = storeId.ToString()[..8].ToUpper();
        return $"INV-{periodStart:yyyy-MM}-{storeIdShort}";
    }

    private string GenerateInvoiceHtml(SellerFinancialSummary summary, string invoiceNumber)
    {
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>Invoice {invoiceNumber}</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; color: #333; }");
        html.AppendLine("        .header { text-align: center; margin-bottom: 40px; }");
        html.AppendLine("        .header h1 { color: #2c3e50; margin-bottom: 10px; }");
        html.AppendLine("        .invoice-info { margin-bottom: 30px; }");
        html.AppendLine("        .invoice-info table { width: 100%; border-collapse: collapse; }");
        html.AppendLine("        .invoice-info td { padding: 8px; border-bottom: 1px solid #eee; }");
        html.AppendLine("        .invoice-info td:first-child { font-weight: bold; width: 200px; }");
        html.AppendLine("        .summary { margin-top: 30px; }");
        html.AppendLine("        .summary table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        html.AppendLine("        .summary th { background-color: #3498db; color: white; padding: 12px; text-align: left; }");
        html.AppendLine("        .summary td { padding: 12px; border-bottom: 1px solid #eee; }");
        html.AppendLine("        .summary tr.total { background-color: #ecf0f1; font-weight: bold; }");
        html.AppendLine("        .summary tr.net { background-color: #27ae60; color: white; font-weight: bold; font-size: 1.1em; }");
        html.AppendLine("        .amount { text-align: right; }");
        html.AppendLine("        .footer { margin-top: 50px; text-align: center; color: #7f8c8d; font-size: 0.9em; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Header
        html.AppendLine("    <div class=\"header\">");
        html.AppendLine("        <h1>Mercato Platform</h1>");
        html.AppendLine("        <h2>Commission Statement</h2>");
        html.AppendLine("    </div>");

        // Invoice Info
        html.AppendLine("    <div class=\"invoice-info\">");
        html.AppendLine("        <table>");
        html.AppendLine($"            <tr><td>Invoice Number:</td><td>{invoiceNumber}</td></tr>");
        html.AppendLine($"            <tr><td>Store Name:</td><td>{summary.StoreName}</td></tr>");
        html.AppendLine($"            <tr><td>Period:</td><td>{summary.PeriodStartDate:yyyy-MM-dd} to {summary.PeriodEndDate:yyyy-MM-dd}</td></tr>");
        html.AppendLine($"            <tr><td>Generated Date:</td><td>{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</td></tr>");
        html.AppendLine($"            <tr><td>Number of Orders:</td><td>{summary.OrderCount}</td></tr>");
        html.AppendLine("        </table>");
        html.AppendLine("    </div>");

        // Summary
        html.AppendLine("    <div class=\"summary\">");
        html.AppendLine("        <h3>Financial Summary</h3>");
        html.AppendLine("        <table>");
        html.AppendLine("            <thead>");
        html.AppendLine("                <tr>");
        html.AppendLine("                    <th>Description</th>");
        html.AppendLine("                    <th class=\"amount\">Amount ({summary.Currency})</th>");
        html.AppendLine("                </tr>");
        html.AppendLine("            </thead>");
        html.AppendLine("            <tbody>");
        html.AppendLine($"                <tr><td>Product Sales</td><td class=\"amount\">{summary.TotalProductValue:N2}</td></tr>");
        html.AppendLine($"                <tr><td>Shipping Fees</td><td class=\"amount\">{summary.TotalShippingFees:N2}</td></tr>");
        html.AppendLine($"                <tr class=\"total\"><td>Gross Merchandise Value (GMV)</td><td class=\"amount\">{summary.TotalGMV:N2}</td></tr>");
        html.AppendLine($"                <tr><td>Platform Commission</td><td class=\"amount\">-{summary.TotalCommission:N2}</td></tr>");
        html.AppendLine($"                <tr><td>Payment Processing Fees</td><td class=\"amount\">-{summary.TotalProcessingFees:N2}</td></tr>");
        html.AppendLine($"                <tr class=\"net\"><td>Net Amount Due to Seller</td><td class=\"amount\">{summary.NetAmountDue:N2}</td></tr>");
        html.AppendLine("            </tbody>");
        html.AppendLine("        </table>");
        html.AppendLine("    </div>");

        // Footer
        html.AppendLine("    <div class=\"footer\">");
        html.AppendLine("        <p>This is a system-generated statement. For questions, please contact support@mercato.com</p>");
        html.AppendLine("        <p>&copy; 2024 Mercato Platform. All rights reserved.</p>");
        html.AppendLine("    </div>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }
}
