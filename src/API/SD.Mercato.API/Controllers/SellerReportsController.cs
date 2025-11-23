using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for seller financial reports and invoices.
/// Provides endpoints for sellers to view their financial summary and download invoices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Seller")]
public class SellerReportsController : ControllerBase
{
    private readonly ISellerReportService _reportService;
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<SellerReportsController> _logger;

    public SellerReportsController(
        ISellerReportService reportService,
        IInvoiceService invoiceService,
        ILogger<SellerReportsController> logger)
    {
        _reportService = reportService;
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets financial summary for the authenticated seller for a specific period.
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <returns>Financial summary with GMV, commissions, and net amount</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetFinancialSummary(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        _logger.LogInformation("Seller {UserId} requesting financial summary for Store {StoreId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), storeId);

        // TODO: Verify that the authenticated user owns this store
        // This would require querying the Store to get OwnerId and comparing with User.Id
        // For now, we trust the storeId parameter but this is a security concern

        var request = new SellerFinancialReportRequest
        {
            StoreId = storeId,
            StartDate = startDate.Date, // Normalize to start of day
            EndDate = endDate.Date.AddDays(1).AddTicks(-1) // End of day
        };

        var summary = await _reportService.GetFinancialSummaryAsync(request);

        if (summary == null)
        {
            return NotFound(new { message = "Store not found or no data available" });
        }

        return Ok(summary);
    }

    /// <summary>
    /// Gets detailed commission breakdown for the authenticated seller.
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <returns>List of commission line items</returns>
    [HttpGet("commission-breakdown")]
    public async Task<IActionResult> GetCommissionBreakdown(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        _logger.LogInformation("Seller {UserId} requesting commission breakdown for Store {StoreId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), storeId);

        // TODO: Verify store ownership

        var request = new SellerFinancialReportRequest
        {
            StoreId = storeId,
            StartDate = startDate.Date,
            EndDate = endDate.Date.AddDays(1).AddTicks(-1)
        };

        var breakdown = await _reportService.GetCommissionBreakdownAsync(request);

        return Ok(breakdown);
    }

    /// <summary>
    /// Generates a monthly invoice for the authenticated seller.
    /// </summary>
    /// <param name="request">Invoice generation request</param>
    /// <returns>Invoice generation response with invoice ID</returns>
    [HttpPost("generate-invoice")]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Seller {UserId} generating invoice for Store {StoreId}",
            userId, request.StoreId);

        // TODO: Verify store ownership

        var requestWithUser = request with { GeneratedBy = userId };

        var response = await _invoiceService.GenerateInvoiceAsync(requestWithUser);

        if (!response.Success)
        {
            return BadRequest(new { message = response.ErrorMessage });
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets all invoices for the authenticated seller's store.
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <returns>List of invoices</returns>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] Guid storeId)
    {
        _logger.LogInformation("Seller {UserId} requesting invoices for Store {StoreId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), storeId);

        // TODO: Verify store ownership

        var invoices = await _invoiceService.GetStoreInvoicesAsync(storeId);

        return Ok(invoices);
    }

    /// <summary>
    /// Downloads an invoice as HTML.
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <returns>HTML content of the invoice</returns>
    [HttpGet("invoices/{invoiceId}/download")]
    public async Task<IActionResult> DownloadInvoice(Guid invoiceId)
    {
        _logger.LogInformation("Seller {UserId} downloading invoice {InvoiceId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), invoiceId);

        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);

        if (invoice == null)
        {
            return NotFound(new { message = "Invoice not found" });
        }

        // TODO: Verify that the invoice belongs to the authenticated seller's store

        var html = await _invoiceService.GetInvoiceHtmlAsync(invoiceId);

        if (string.IsNullOrEmpty(html))
        {
            return NotFound(new { message = "Invoice content not available" });
        }

        return Content(html, "text/html");
    }
}
