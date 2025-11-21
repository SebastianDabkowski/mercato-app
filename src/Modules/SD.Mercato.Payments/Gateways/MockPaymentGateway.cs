using Microsoft.Extensions.Logging;

namespace SD.Mercato.Payments.Gateways;

/// <summary>
/// Mock payment gateway implementation for development and testing.
/// This is a stub that simulates successful payments without actual gateway integration.
/// 
/// TODO: Replace with actual payment gateway implementation (Stripe, PayU, etc.) before production.
/// TODO: Implement proper error handling and retry logic for production use.
/// TODO: Add support for webhooks from the actual gateway.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private readonly Dictionary<string, MockPaymentSession> _sessions = new();

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentSessionResponse> CreatePaymentSessionAsync(CreatePaymentSessionRequest request)
    {
        _logger.LogInformation("Creating mock payment session for Order {OrderId}, Amount {Amount}",
            request.OrderId, request.Amount);

        // Generate a mock session ID
        var sessionId = $"mock_session_{Guid.NewGuid():N}";
        
        // Store session for later retrieval
        _sessions[sessionId] = new MockPaymentSession
        {
            SessionId = sessionId,
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            CustomerEmail = request.CustomerEmail,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // In a real implementation, this would be the actual gateway checkout URL
        // For mock, we'll use a placeholder that could redirect to a test payment page
        var checkoutUrl = $"{request.SuccessUrl}?session_id={sessionId}&mock=true";

        _logger.LogInformation("Mock payment session created: {SessionId}, CheckoutUrl: {CheckoutUrl}",
            sessionId, checkoutUrl);

        return Task.FromResult(new PaymentSessionResponse
        {
            SessionId = sessionId,
            CheckoutUrl = checkoutUrl,
            Success = true
        });
    }

    public Task<PaymentStatusResponse> GetPaymentStatusAsync(string sessionId)
    {
        _logger.LogInformation("Checking payment status for session {SessionId}", sessionId);

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Payment session not found: {SessionId}", sessionId);
            return Task.FromResult(new PaymentStatusResponse
            {
                Status = "Failed"
            });
        }

        // TODO: In production, this would query the actual payment gateway
        // For mock, auto-complete payments after a brief delay simulation
        if (session.Status == "Pending" && (DateTime.UtcNow - session.CreatedAt).TotalSeconds > 2)
        {
            session.Status = "Completed";
            session.TransactionId = $"mock_txn_{Guid.NewGuid():N}";
            
            _logger.LogInformation("Mock payment auto-completed for session {SessionId}", sessionId);
        }

        return Task.FromResult(new PaymentStatusResponse
        {
            Status = session.Status,
            TransactionId = session.TransactionId,
            PaymentMethod = "MockCard",
            Amount = session.Amount,
            Metadata = new Dictionary<string, string>
            {
                { "mock", "true" },
                { "customer_email", session.CustomerEmail }
            }
        });
    }

    public bool ValidateWebhookSignature(string payload, string signature, string secret)
    {
        // TODO: Implement actual signature validation for production gateway
        // For mock, always return true in development
        _logger.LogWarning("Mock webhook signature validation - always returns true. " +
                          "Implement proper validation before production!");
        return true;
    }

    public Task<RefundResponse> RefundPaymentAsync(string transactionId, decimal amount, string? reason)
    {
        _logger.LogInformation("Processing mock refund for transaction {TransactionId}, Amount {Amount}, Reason: {Reason}",
            transactionId, amount, reason);

        // TODO: Implement actual refund logic with real payment gateway
        // For mock, simulate successful refund
        return Task.FromResult(new RefundResponse
        {
            Success = true,
            RefundId = $"mock_refund_{Guid.NewGuid():N}"
        });
    }

    public Task<bool> CancelPaymentSessionAsync(string sessionId)
    {
        _logger.LogInformation("Cancelling mock payment session {SessionId}", sessionId);

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Status = "Cancelled";
            _logger.LogInformation("Mock payment session cancelled: {SessionId}", sessionId);
            return Task.FromResult(true);
        }

        _logger.LogWarning("Payment session not found for cancellation: {SessionId}", sessionId);
        return Task.FromResult(false);
    }

    private class MockPaymentSession
    {
        public required string SessionId { get; init; }
        public required Guid OrderId { get; init; }
        public required decimal Amount { get; init; }
        public required string Currency { get; init; }
        public required string CustomerEmail { get; init; }
        public required string Status { get; set; }
        public string? TransactionId { get; set; }
        public required DateTime CreatedAt { get; init; }
    }
}
