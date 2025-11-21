using SD.Mercato.Payments.DTOs;
using SD.Mercato.Payments.Models;

namespace SD.Mercato.Payments.Services;

/// <summary>
/// Service interface for payment operations.
/// Handles payment initiation, confirmation, commission calculation, and seller balance management.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initiates a payment session for an order.
    /// Creates a payment transaction record and returns gateway checkout URL.
    /// </summary>
    Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request);

    /// <summary>
    /// Confirms a payment after gateway callback or webhook.
    /// Updates payment transaction status and triggers order confirmation.
    /// </summary>
    Task<ConfirmPaymentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request);

    /// <summary>
    /// Cancels a pending payment session.
    /// </summary>
    Task<bool> CancelPaymentAsync(string sessionId);

    /// <summary>
    /// Calculates commission and fees for a SubOrder.
    /// </summary>
    Task<CommissionCalculation> CalculateCommissionAsync(
        decimal productTotal,
        decimal shippingCost,
        decimal commissionRate,
        decimal totalOrderAmount,
        decimal totalProcessingFee);

    /// <summary>
    /// Records SubOrder payment details after successful order payment.
    /// Creates SubOrderPayment records and updates seller balances.
    /// </summary>
    Task RecordSubOrderPaymentsAsync(
        Guid paymentTransactionId,
        Guid orderId,
        decimal totalProcessingFee);

    /// <summary>
    /// Gets the current balance for a seller/store.
    /// </summary>
    Task<SellerBalanceResponse?> GetSellerBalanceAsync(Guid storeId);

    /// <summary>
    /// Releases funds from escrow when a SubOrder is delivered.
    /// Moves amount from pending to available balance.
    /// </summary>
    Task<bool> ReleaseEscrowForSubOrderAsync(Guid subOrderId);

    /// <summary>
    /// Creates a payout for a seller.
    /// </summary>
    Task<CreatePayoutResponse> CreatePayoutAsync(CreatePayoutRequest request);

    /// <summary>
    /// Gets payment transaction details.
    /// </summary>
    Task<PaymentTransaction?> GetPaymentTransactionAsync(Guid transactionId);

    /// <summary>
    /// Gets payment transaction by order ID.
    /// </summary>
    Task<PaymentTransaction?> GetPaymentTransactionByOrderIdAsync(Guid orderId);
}
