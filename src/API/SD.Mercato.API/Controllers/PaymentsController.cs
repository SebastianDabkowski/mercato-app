using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Payments.DTOs;
using SD.Mercato.Payments.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for payment operations.
/// Handles payment initiation, confirmation, and seller balance queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a payment session for an order.
    /// Returns a checkout URL to redirect the buyer to the payment gateway.
    /// </summary>
    [HttpPost("initiate")]
    [Authorize]
    [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InitiatePaymentResponse>> InitiatePayment([FromBody] InitiatePaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation("User {UserId} initiating payment for Order {OrderId}", userId, request.OrderId);

        var result = await _paymentService.InitiatePaymentAsync(request);

        if (!result.Success)
        {
            _logger.LogWarning("Payment initiation failed for Order {OrderId}: {Error}",
                request.OrderId, result.ErrorMessage);
            return BadRequest(result);
        }

        _logger.LogInformation("Payment initiated successfully: SessionId={SessionId}, TransactionId={TransactionId}",
            result.SessionId, result.TransactionId);

        return Ok(result);
    }

    /// <summary>
    /// Confirms payment after gateway callback.
    /// This endpoint should be called by the payment gateway webhook or after redirect.
    /// </summary>
    /// <remarks>
    /// TODO: SECURITY CRITICAL - Implement webhook signature validation before production.
    /// Without signature validation, this endpoint is vulnerable to fraudulent confirmations.
    /// </remarks>
    [HttpPost("confirm")]
    [AllowAnonymous] // Payment gateway may call this endpoint
    [ProducesResponseType(typeof(ConfirmPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConfirmPaymentResponse>> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        // TODO: Validate webhook signature from payment gateway
        // Example for Stripe:
        // var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        // if (!_paymentGateway.ValidateWebhookSignature(rawBody, signature, webhookSecret))
        // {
        //     return Unauthorized(new { message = "Invalid webhook signature" });
        // }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Processing payment confirmation for SessionId {SessionId}", request.SessionId);

        var result = await _paymentService.ConfirmPaymentAsync(request);

        if (!result.Success)
        {
            _logger.LogWarning("Payment confirmation failed for SessionId {SessionId}: {Error}",
                request.SessionId, result.ErrorMessage);
            return BadRequest(result);
        }

        _logger.LogInformation("Payment confirmed successfully for Order {OrderId}", result.OrderId);

        return Ok(result);
    }

    /// <summary>
    /// Cancels a pending payment session.
    /// </summary>
    [HttpPost("cancel/{sessionId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(string sessionId)
    {
        _logger.LogInformation("Cancelling payment session {SessionId}", sessionId);

        var cancelled = await _paymentService.CancelPaymentAsync(sessionId);

        if (!cancelled)
        {
            return NotFound(new { message = "Payment session not found or already processed" });
        }

        return Ok(new { message = "Payment cancelled successfully" });
    }

    /// <summary>
    /// Gets the current balance for a seller/store.
    /// Only accessible by the store owner or administrators.
    /// </summary>
    [HttpGet("balance/{storeId}")]
    [Authorize(Roles = "Seller,Administrator")]
    [ProducesResponseType(typeof(SellerBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SellerBalanceResponse>> GetSellerBalance(Guid storeId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Administrator");

        // TODO: Verify that the authenticated user owns this store
        // This requires integration with SellerPanel module to check store ownership
        // For now, we trust the authorization attribute

        var balance = await _paymentService.GetSellerBalanceAsync(storeId);

        if (balance == null)
        {
            return NotFound(new { message = "Seller balance not found" });
        }

        return Ok(balance);
    }

    /// <summary>
    /// Creates a payout for a seller.
    /// Transfers available funds from seller balance to their bank account.
    /// </summary>
    [HttpPost("payout")]
    [Authorize(Roles = "Seller,Administrator")]
    [ProducesResponseType(typeof(CreatePayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatePayoutResponse>> CreatePayout([FromBody] CreatePayoutRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // TODO: Verify that the authenticated user owns this store
        // This requires integration with SellerPanel module

        _logger.LogInformation("Creating payout for Store {StoreId}, Amount {Amount}", 
            request.StoreId, request.Amount);

        var result = await _paymentService.CreatePayoutAsync(request);

        if (!result.Success)
        {
            _logger.LogWarning("Payout creation failed for Store {StoreId}: {Error}",
                request.StoreId, result.ErrorMessage);
            return BadRequest(result);
        }

        _logger.LogInformation("Payout created successfully: PayoutId={PayoutId}", result.PayoutId);

        return Ok(result);
    }

    /// <summary>
    /// Releases escrow funds for a delivered SubOrder.
    /// Called when a SubOrder is marked as delivered.
    /// </summary>
    /// <remarks>
    /// This is typically called internally by the Order management system,
    /// not directly by users.
    /// </remarks>
    [HttpPost("release-escrow/{subOrderId}")]
    [Authorize(Roles = "Administrator")] // Only admin can manually trigger this
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleaseEscrow(Guid subOrderId)
    {
        _logger.LogInformation("Releasing escrow for SubOrder {SubOrderId}", subOrderId);

        var released = await _paymentService.ReleaseEscrowForSubOrderAsync(subOrderId);

        if (!released)
        {
            return BadRequest(new { message = "Failed to release escrow. SubOrder may not exist or is not in correct status." });
        }

        return Ok(new { message = "Escrow released successfully" });
    }

    /// <summary>
    /// Gets payment transaction details.
    /// </summary>
    [HttpGet("transaction/{transactionId}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(Guid transactionId)
    {
        var transaction = await _paymentService.GetPaymentTransactionAsync(transactionId);

        if (transaction == null)
        {
            return NotFound(new { message = "Transaction not found" });
        }

        // TODO: Verify user has permission to view this transaction
        // (either owns the order or is admin)

        return Ok(transaction);
    }
}
