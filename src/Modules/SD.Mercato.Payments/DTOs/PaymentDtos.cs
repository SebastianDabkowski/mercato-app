namespace SD.Mercato.Payments.DTOs;

/// <summary>
/// Request to initiate a payment for an order.
/// </summary>
public record InitiatePaymentRequest
{
    /// <summary>
    /// Order ID to create payment for.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Success redirect URL after payment completion.
    /// </summary>
    public required string SuccessUrl { get; init; }

    /// <summary>
    /// Cancel redirect URL if buyer cancels payment.
    /// </summary>
    public required string CancelUrl { get; init; }
}

/// <summary>
/// Response from initiating a payment.
/// </summary>
public record InitiatePaymentResponse
{
    /// <summary>
    /// Whether payment session was created successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Payment session ID from gateway.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// URL to redirect buyer for payment.
    /// </summary>
    public string? CheckoutUrl { get; init; }

    /// <summary>
    /// Payment transaction ID created in our system.
    /// </summary>
    public Guid? TransactionId { get; init; }

    /// <summary>
    /// Error message if initiation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Request to confirm payment completion (called by webhook or callback).
/// </summary>
public record ConfirmPaymentRequest
{
    /// <summary>
    /// Payment gateway session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Payment status from gateway.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gateway transaction ID (if successful).
    /// </summary>
    public string? GatewayTransactionId { get; init; }
}

/// <summary>
/// Response from confirming payment.
/// </summary>
public record ConfirmPaymentResponse
{
    /// <summary>
    /// Whether confirmation was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Order ID that was paid.
    /// </summary>
    public Guid? OrderId { get; init; }

    /// <summary>
    /// Error message if confirmation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Commission calculation result for an order or SubOrder.
/// </summary>
public record CommissionCalculation
{
    /// <summary>
    /// Product subtotal (base for commission calculation).
    /// </summary>
    public required decimal ProductTotal { get; init; }

    /// <summary>
    /// Shipping cost (commission NOT applied to this).
    /// </summary>
    public required decimal ShippingCost { get; init; }

    /// <summary>
    /// Total amount (ProductTotal + ShippingCost).
    /// </summary>
    public required decimal Total { get; init; }

    /// <summary>
    /// Commission rate applied (e.g., 0.15 for 15%).
    /// </summary>
    public required decimal CommissionRate { get; init; }

    /// <summary>
    /// Commission amount (ProductTotal × CommissionRate).
    /// </summary>
    public required decimal CommissionAmount { get; init; }

    /// <summary>
    /// Processing fee allocated to this portion.
    /// </summary>
    public required decimal ProcessingFee { get; init; }

    /// <summary>
    /// Net amount seller receives (Total - CommissionAmount - ProcessingFee).
    /// </summary>
    public required decimal SellerNetAmount { get; init; }
}

/// <summary>
/// Response for querying seller balance.
/// </summary>
public record SellerBalanceResponse
{
    /// <summary>
    /// Store ID.
    /// </summary>
    public required Guid StoreId { get; init; }

    /// <summary>
    /// Pending amount (in escrow, not yet available).
    /// </summary>
    public required decimal PendingAmount { get; init; }

    /// <summary>
    /// Available amount ready for payout.
    /// </summary>
    public required decimal AvailableAmount { get; init; }

    /// <summary>
    /// Total already paid out to seller.
    /// </summary>
    public required decimal TotalPaidOut { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public required DateTime LastUpdatedAt { get; init; }
}

/// <summary>
/// Request to create a payout for a seller.
/// </summary>
public record CreatePayoutRequest
{
    /// <summary>
    /// Store ID to pay out.
    /// </summary>
    public required Guid StoreId { get; init; }

    /// <summary>
    /// Amount to pay out (must not exceed available balance).
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Payout method (e.g., "BankTransfer").
    /// </summary>
    public string PayoutMethod { get; init; } = "BankTransfer";
}

/// <summary>
/// Response from creating a payout.
/// </summary>
public record CreatePayoutResponse
{
    /// <summary>
    /// Whether payout was created successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Payout ID.
    /// </summary>
    public Guid? PayoutId { get; init; }

    /// <summary>
    /// Error message if creation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
