namespace SD.Mercato.Payments.Gateways;

/// <summary>
/// Abstraction for payment gateway integration.
/// Allows swapping payment providers (Stripe, PayU, etc.) without changing core business logic.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Creates a payment session/checkout for the specified order.
    /// Returns a session ID and redirect URL for the buyer to complete payment.
    /// </summary>
    /// <param name="request">Payment session creation request.</param>
    /// <returns>Payment session response with session ID and checkout URL.</returns>
    Task<PaymentSessionResponse> CreatePaymentSessionAsync(CreatePaymentSessionRequest request);

    /// <summary>
    /// Retrieves the status of a payment from the gateway.
    /// Used to verify payment completion and synchronize state.
    /// </summary>
    /// <param name="sessionId">Payment gateway session ID.</param>
    /// <returns>Current payment status.</returns>
    Task<PaymentStatusResponse> GetPaymentStatusAsync(string sessionId);

    /// <summary>
    /// Validates a webhook signature from the payment gateway.
    /// Ensures webhook requests are authentic and not forged.
    /// </summary>
    /// <param name="payload">Raw webhook payload.</param>
    /// <param name="signature">Signature header from the webhook request.</param>
    /// <param name="secret">Webhook signing secret.</param>
    /// <returns>True if signature is valid, false otherwise.</returns>
    bool ValidateWebhookSignature(string payload, string signature, string secret);

    /// <summary>
    /// Processes a refund for a completed payment.
    /// </summary>
    /// <param name="transactionId">Original payment transaction ID.</param>
    /// <param name="amount">Amount to refund (partial or full).</param>
    /// <param name="reason">Reason for the refund.</param>
    /// <returns>Refund result.</returns>
    Task<RefundResponse> RefundPaymentAsync(string transactionId, decimal amount, string? reason);

    /// <summary>
    /// Cancels a pending payment session before it's completed.
    /// </summary>
    /// <param name="sessionId">Payment session ID to cancel.</param>
    /// <returns>True if successfully cancelled, false otherwise.</returns>
    Task<bool> CancelPaymentSessionAsync(string sessionId);
}

/// <summary>
/// Request for creating a payment session.
/// </summary>
public record CreatePaymentSessionRequest
{
    /// <summary>
    /// Internal order ID for reference.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Total amount to charge.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code (e.g., "USD").
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Customer email address.
    /// </summary>
    public required string CustomerEmail { get; init; }

    /// <summary>
    /// URL to redirect on successful payment.
    /// </summary>
    public required string SuccessUrl { get; init; }

    /// <summary>
    /// URL to redirect on payment cancellation.
    /// </summary>
    public required string CancelUrl { get; init; }

    /// <summary>
    /// Additional metadata to attach to the payment session.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Response from creating a payment session.
/// </summary>
public record PaymentSessionResponse
{
    /// <summary>
    /// Payment session ID from the gateway.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// URL to redirect the buyer for payment.
    /// </summary>
    public required string CheckoutUrl { get; init; }

    /// <summary>
    /// Whether the session was successfully created.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Error message if creation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Response from checking payment status.
/// </summary>
public record PaymentStatusResponse
{
    /// <summary>
    /// Payment status: Pending, Completed, Failed, Cancelled.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gateway transaction ID (if payment completed).
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string? PaymentMethod { get; init; }

    /// <summary>
    /// Amount paid.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Additional metadata from the gateway.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Response from refund operation.
/// </summary>
public record RefundResponse
{
    /// <summary>
    /// Whether the refund was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Refund transaction ID from gateway.
    /// </summary>
    public string? RefundId { get; init; }

    /// <summary>
    /// Error message if refund failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
