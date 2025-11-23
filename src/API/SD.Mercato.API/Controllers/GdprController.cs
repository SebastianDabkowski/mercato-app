using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Users.DTOs;
using SD.Mercato.Users.Services;
using System.Text;
using System.Text.Json;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for GDPR compliance operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GdprController : ControllerBase
{
    private readonly IGdprService _gdprService;
    private readonly ILogger<GdprController> _logger;

    public GdprController(IGdprService gdprService, ILogger<GdprController> logger)
    {
        _gdprService = gdprService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's consent settings.
    /// GDPR: Transparency - user can view their consent status.
    /// </summary>
    [HttpGet("consent")]
    [ProducesResponseType(typeof(ConsentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsentResponse>> GetConsent()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var consent = await _gdprService.GetConsentAsync(userId);
        if (consent == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(consent);
    }

    /// <summary>
    /// Update user's email marketing consent.
    /// GDPR: Right to withdraw consent (Article 7).
    /// </summary>
    [HttpPut("consent/email-marketing")]
    [ProducesResponseType(typeof(ConsentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsentResponse>> UpdateEmailMarketingConsent([FromBody] UpdateConsentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _gdprService.UpdateEmailMarketingConsentAsync(userId, request);
            _logger.LogInformation(
                "User {UserId} updated email marketing consent to {Consent}",
                userId,
                request.EmailMarketingConsent);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consent for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export all user data.
    /// GDPR: Right to data portability (Article 20).
    /// Returns data in JSON or CSV format.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportData([FromBody] DataExportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var format = request.Format.ToLowerInvariant();
        if (format != "json" && format != "csv")
        {
            return BadRequest(new { message = "Invalid format. Supported formats: json, csv" });
        }

        var exportData = await _gdprService.ExportUserDataAsync(userId, request);
        if (exportData == null)
        {
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("User {UserId} exported their data in {Format} format", userId, format);

        if (format == "json")
        {
            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return File(jsonBytes, "application/json", $"user_data_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        }
        else // csv
        {
            var csv = ConvertToCsv(exportData.Data);
            var csvBytes = Encoding.UTF8.GetBytes(csv);
            return File(csvBytes, "text/csv", $"user_data_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
    }

    /// <summary>
    /// Delete user account and anonymize personal data.
    /// GDPR: Right to erasure (Article 17).
    /// Anonymizes personal data while retaining records for legal/accounting requirements.
    /// </summary>
    [HttpDelete("account")]
    [ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeleteAccountResponse>> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _gdprService.DeleteUserAccountAsync(userId, request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogWarning(
            "User {UserId} deleted their account. Reason: {Reason}",
            userId,
            request.Reason ?? "Not specified");

        return Ok(result);
    }

    /// <summary>
    /// Converts user data export to CSV format.
    /// </summary>
    private string ConvertToCsv(UserDataExport data)
    {
        var sb = new StringBuilder();

        // Profile section
        sb.AppendLine("PROFILE DATA");
        sb.AppendLine("Field,Value");
        sb.AppendLine($"Email,\"{data.Profile.Email}\"");
        sb.AppendLine($"First Name,\"{data.Profile.FirstName}\"");
        sb.AppendLine($"Last Name,\"{data.Profile.LastName}\"");
        sb.AppendLine($"Phone Number,\"{data.Profile.PhoneNumber ?? "N/A"}\"");
        sb.AppendLine($"Account Created,\"{data.Profile.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        sb.AppendLine();

        // Consent section
        sb.AppendLine("CONSENT DATA");
        sb.AppendLine("Field,Value");
        sb.AppendLine($"Email Marketing Consent,{data.Consent.EmailMarketingConsent}");
        sb.AppendLine($"Consent Updated At,\"{data.Consent.EmailMarketingConsentUpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}\"");
        sb.AppendLine();

        // Account section
        sb.AppendLine("ACCOUNT DATA");
        sb.AppendLine("Field,Value");
        sb.AppendLine($"Created At,\"{data.Account.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        sb.AppendLine($"Last Login,\"{data.Account.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}\"");
        sb.AppendLine($"Email Verified,{data.Account.IsEmailVerified}");
        sb.AppendLine($"External Provider,\"{data.Account.ExternalProvider ?? "N/A"}\"");
        sb.AppendLine();

        // Orders section
        if (data.Orders.Any())
        {
            sb.AppendLine("ORDER HISTORY");
            sb.AppendLine("Order Number,Order Date,Total Amount,Currency,Status,Payment Status");
            foreach (var order in data.Orders)
            {
                sb.AppendLine($"\"{order.OrderNumber}\",\"{order.OrderDate:yyyy-MM-dd HH:mm:ss}\",{order.TotalAmount},\"{order.Currency}\",\"{order.Status}\",\"{order.PaymentStatus}\"");
            }
        }
        else
        {
            sb.AppendLine("ORDER HISTORY");
            sb.AppendLine("No orders found");
        }

        return sb.ToString();
    }
}
