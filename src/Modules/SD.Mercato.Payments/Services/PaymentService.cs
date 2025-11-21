using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SD.Mercato.Payments.Data;
using SD.Mercato.Payments.DTOs;
using SD.Mercato.Payments.Gateways;
using SD.Mercato.Payments.Models;

namespace SD.Mercato.Payments.Services;

/// <summary>
/// Implementation of payment service.
/// Handles payment flow, commission calculation, and seller balance management.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly PaymentsDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    // Payment gateway fee configuration
    private const decimal DefaultProcessingFeeRate = 0.029m; // 2.9%
    private const decimal DefaultProcessingFeeFixed = 0.30m; // $0.30
    private const decimal DefaultCommissionRate = 0.15m; // 15%

    public PaymentService(
        PaymentsDbContext context,
        IPaymentGateway paymentGateway,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
    {
        _logger.LogInformation("Initiating payment for Order {OrderId}", request.OrderId);

        // TODO: Get order details from History module to verify amount and get customer email
        // For now, this is a placeholder - actual implementation needs to query Order service
        // var order = await _orderService.GetOrderByIdAsync(request.OrderId);
        // if (order == null) return error response

        // TODO: Calculate total amount from order (this should come from the order)
        decimal orderTotal = 100m; // Placeholder
        string customerEmail = "customer@example.com"; // Placeholder

        try
        {
            // Calculate processing fee
            var processingFee = CalculateProcessingFee(orderTotal);

            // Create payment transaction record
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                Amount = orderTotal,
                Currency = "USD",
                Status = PaymentTransactionStatus.Pending,
                ProcessingFee = processingFee,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment transaction created: {TransactionId}", transaction.Id);

            // Create payment session with gateway
            var gatewayRequest = new CreatePaymentSessionRequest
            {
                OrderId = request.OrderId,
                Amount = orderTotal,
                Currency = "USD",
                CustomerEmail = customerEmail,
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() },
                    { "transaction_id", transaction.Id.ToString() }
                }
            };

            var gatewayResponse = await _paymentGateway.CreatePaymentSessionAsync(gatewayRequest);

            if (!gatewayResponse.Success)
            {
                _logger.LogError("Payment gateway failed to create session for Order {OrderId}: {Error}",
                    request.OrderId, gatewayResponse.ErrorMessage);

                transaction.Status = PaymentTransactionStatus.Failed;
                transaction.ErrorMessage = gatewayResponse.ErrorMessage;
                await _context.SaveChangesAsync();

                return new InitiatePaymentResponse
                {
                    Success = false,
                    ErrorMessage = gatewayResponse.ErrorMessage ?? "Failed to create payment session"
                };
            }

            // Update transaction with session details
            transaction.PaymentGatewaySessionId = gatewayResponse.SessionId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment session created: SessionId={SessionId}, CheckoutUrl={CheckoutUrl}",
                gatewayResponse.SessionId, gatewayResponse.CheckoutUrl);

            return new InitiatePaymentResponse
            {
                Success = true,
                SessionId = gatewayResponse.SessionId,
                CheckoutUrl = gatewayResponse.CheckoutUrl,
                TransactionId = transaction.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for Order {OrderId}", request.OrderId);
            return new InitiatePaymentResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while initiating payment"
            };
        }
    }

    public async Task<ConfirmPaymentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        _logger.LogInformation("Confirming payment for SessionId {SessionId}", request.SessionId);

        try
        {
            // Find transaction by session ID
            var transaction = await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.PaymentGatewaySessionId == request.SessionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for SessionId {SessionId}", request.SessionId);
                return new ConfirmPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Transaction not found"
                };
            }

            // Update transaction status
            transaction.Status = request.Status == "Completed" 
                ? PaymentTransactionStatus.Completed 
                : PaymentTransactionStatus.Failed;
            transaction.PaymentGatewayTransactionId = request.GatewayTransactionId;
            transaction.CompletedAt = DateTime.UtcNow;

            // TODO: Get payment method from gateway response
            transaction.PaymentMethod = "CreditCard"; // Placeholder

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment transaction updated: {TransactionId}, Status={Status}",
                transaction.Id, transaction.Status);

            // If payment successful, record SubOrder payments and update balances
            if (transaction.Status == PaymentTransactionStatus.Completed)
            {
                await RecordSubOrderPaymentsAsync(
                    transaction.Id,
                    transaction.OrderId,
                    transaction.ProcessingFee);
            }

            return new ConfirmPaymentResponse
            {
                Success = true,
                OrderId = transaction.OrderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for SessionId {SessionId}", request.SessionId);
            return new ConfirmPaymentResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while confirming payment"
            };
        }
    }

    public async Task<bool> CancelPaymentAsync(string sessionId)
    {
        _logger.LogInformation("Cancelling payment for SessionId {SessionId}", sessionId);

        try
        {
            var transaction = await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.PaymentGatewaySessionId == sessionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for SessionId {SessionId}", sessionId);
                return false;
            }

            // Cancel at gateway
            var cancelled = await _paymentGateway.CancelPaymentSessionAsync(sessionId);

            if (cancelled)
            {
                transaction.Status = PaymentTransactionStatus.Cancelled;
                transaction.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment cancelled: {TransactionId}", transaction.Id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment for SessionId {SessionId}", sessionId);
            return false;
        }
    }

    public Task<CommissionCalculation> CalculateCommissionAsync(
        decimal productTotal,
        decimal shippingCost,
        decimal commissionRate,
        decimal totalOrderAmount,
        decimal totalProcessingFee)
    {
        // Validate inputs to prevent division by zero and negative values
        if (totalOrderAmount <= 0)
        {
            throw new ArgumentException("Total order amount must be greater than zero", nameof(totalOrderAmount));
        }

        if (productTotal < 0 || shippingCost < 0)
        {
            throw new ArgumentException("Product total and shipping cost cannot be negative");
        }

        if (commissionRate < 0 || commissionRate > 1)
        {
            throw new ArgumentException("Commission rate must be between 0 and 1", nameof(commissionRate));
        }

        var total = productTotal + shippingCost;
        
        // Commission applied only to product total, not shipping
        var commissionAmount = productTotal * commissionRate;
        
        // Processing fee allocated proportionally to this SubOrder's share of the total order
        var processingFeeAllocated = total / totalOrderAmount * totalProcessingFee;
        
        // Net amount seller receives
        var sellerNetAmount = total - commissionAmount - processingFeeAllocated;

        var calculation = new CommissionCalculation
        {
            ProductTotal = productTotal,
            ShippingCost = shippingCost,
            Total = total,
            CommissionRate = commissionRate,
            CommissionAmount = Math.Round(commissionAmount, 2),
            ProcessingFee = Math.Round(processingFeeAllocated, 2),
            SellerNetAmount = Math.Round(sellerNetAmount, 2)
        };

        _logger.LogDebug("Commission calculated: ProductTotal={ProductTotal}, ShippingCost={ShippingCost}, " +
                        "Commission={Commission}, ProcessingFee={ProcessingFee}, Net={Net}",
            productTotal, shippingCost, calculation.CommissionAmount, 
            calculation.ProcessingFee, calculation.SellerNetAmount);

        return Task.FromResult(calculation);
    }

    public async Task RecordSubOrderPaymentsAsync(
        Guid paymentTransactionId,
        Guid orderId,
        decimal totalProcessingFee)
    {
        _logger.LogInformation("Recording SubOrder payments for Order {OrderId}, Transaction {TransactionId}",
            orderId, paymentTransactionId);

        // TODO: Get SubOrders from History module
        // For now, this is a placeholder implementation
        // var subOrders = await _orderService.GetSubOrdersByOrderIdAsync(orderId);

        // Placeholder: Would iterate through actual SubOrders
        // foreach (var subOrder in subOrders)
        // {
        //     var commission = await CalculateCommissionAsync(...);
        //     
        //     var subOrderPayment = new SubOrderPayment { ... };
        //     _context.SubOrderPayments.Add(subOrderPayment);
        //     
        //     await UpdateSellerBalanceAsync(subOrder.StoreId, commission.SellerNetAmount, isPending: true);
        // }

        _logger.LogInformation("SubOrder payments recorded for Order {OrderId}", orderId);
        await _context.SaveChangesAsync();
    }

    public async Task<SellerBalanceResponse?> GetSellerBalanceAsync(Guid storeId)
    {
        var balance = await _context.SellerBalances
            .FirstOrDefaultAsync(b => b.StoreId == storeId);

        if (balance == null)
        {
            // Create initial balance record if doesn't exist
            balance = new SellerBalance
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                PendingAmount = 0,
                AvailableAmount = 0,
                TotalPaidOut = 0,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.SellerBalances.Add(balance);
            await _context.SaveChangesAsync();
        }

        return new SellerBalanceResponse
        {
            StoreId = balance.StoreId,
            PendingAmount = balance.PendingAmount,
            AvailableAmount = balance.AvailableAmount,
            TotalPaidOut = balance.TotalPaidOut,
            Currency = balance.Currency,
            LastUpdatedAt = balance.LastUpdatedAt
        };
    }

    public async Task<bool> ReleaseEscrowForSubOrderAsync(Guid subOrderId)
    {
        _logger.LogInformation("Releasing escrow for SubOrder {SubOrderId}", subOrderId);

        try
        {
            var subOrderPayment = await _context.SubOrderPayments
                .FirstOrDefaultAsync(sp => sp.SubOrderId == subOrderId);

            if (subOrderPayment == null)
            {
                _logger.LogWarning("SubOrderPayment not found for SubOrder {SubOrderId}", subOrderId);
                return false;
            }

            if (subOrderPayment.PayoutStatus != SubOrderPayoutStatus.Pending)
            {
                _logger.LogWarning("SubOrder {SubOrderId} is not in Pending status: {Status}",
                    subOrderId, subOrderPayment.PayoutStatus);
                return false;
            }

            // Update payout status to Released
            subOrderPayment.PayoutStatus = SubOrderPayoutStatus.Released;
            subOrderPayment.UpdatedAt = DateTime.UtcNow;

            // Update seller balance: move from pending to available
            var balance = await _context.SellerBalances
                .FirstOrDefaultAsync(b => b.StoreId == subOrderPayment.StoreId);

            if (balance != null)
            {
                balance.PendingAmount -= subOrderPayment.SellerNetAmount;
                balance.AvailableAmount += subOrderPayment.SellerNetAmount;
                balance.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Escrow released for SubOrder {SubOrderId}, Amount {Amount} moved to available",
                subOrderId, subOrderPayment.SellerNetAmount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing escrow for SubOrder {SubOrderId}", subOrderId);
            return false;
        }
    }

    public async Task<CreatePayoutResponse> CreatePayoutAsync(CreatePayoutRequest request)
    {
        _logger.LogInformation("Creating payout for Store {StoreId}, Amount {Amount}",
            request.StoreId, request.Amount);

        try
        {
            var balance = await _context.SellerBalances
                .FirstOrDefaultAsync(b => b.StoreId == request.StoreId);

            if (balance == null || balance.AvailableAmount < request.Amount)
            {
                return new CreatePayoutResponse
                {
                    Success = false,
                    ErrorMessage = "Insufficient available balance"
                };
            }

            // Get all released SubOrderPayments for this store that haven't been paid out
            // TODO: For stores with very large numbers of SubOrders, implement pagination
            // or batch processing to avoid loading too many records in memory
            var subOrderPayments = await _context.SubOrderPayments
                .Where(sp => sp.StoreId == request.StoreId 
                    && sp.PayoutStatus == SubOrderPayoutStatus.Released)
                .OrderBy(sp => sp.CreatedAt)
                .Take(100) // Limit for safety - make configurable in production
                .ToListAsync();

            var totalAmount = subOrderPayments.Sum(sp => sp.SellerNetAmount);
            
            if (totalAmount < request.Amount)
            {
                return new CreatePayoutResponse
                {
                    Success = false,
                    ErrorMessage = "Requested amount exceeds available funds from released orders"
                };
            }

            // Create payout record
            // TODO: Consider using a separate PayoutSubOrder junction table instead of comma-separated IDs
            // for better data integrity and querying capabilities
            var payout = new Payout
            {
                Id = Guid.NewGuid(),
                StoreId = request.StoreId,
                Amount = request.Amount,
                GrossAmount = subOrderPayments.Sum(sp => sp.SubOrderTotal),
                CommissionAmount = subOrderPayments.Sum(sp => sp.CommissionAmount),
                ProcessingFeeAmount = subOrderPayments.Sum(sp => sp.ProcessingFeeAllocated),
                Currency = "USD",
                Status = PayoutStatus.Pending,
                PayoutMethod = request.PayoutMethod,
                SubOrderIds = string.Join(",", subOrderPayments.Select(sp => sp.SubOrderId)),
                ScheduledAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payouts.Add(payout);

            // Update SubOrderPayments to mark as paid out
            foreach (var sp in subOrderPayments)
            {
                sp.PayoutStatus = SubOrderPayoutStatus.PaidOut;
                sp.PayoutId = payout.Id;
                sp.UpdatedAt = DateTime.UtcNow;
            }

            // Update seller balance
            balance.AvailableAmount -= request.Amount;
            balance.TotalPaidOut += request.Amount;
            balance.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payout created: {PayoutId}, Store {StoreId}, Amount {Amount}",
                payout.Id, request.StoreId, request.Amount);

            return new CreatePayoutResponse
            {
                Success = true,
                PayoutId = payout.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payout for Store {StoreId}", request.StoreId);
            return new CreatePayoutResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while creating payout"
            };
        }
    }

    public async Task<PaymentTransaction?> GetPaymentTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        return transaction;
    }

    public async Task<PaymentTransaction?> GetPaymentTransactionByOrderIdAsync(Guid orderId)
    {
        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.OrderId == orderId);

        return transaction;
    }

    private decimal CalculateProcessingFee(decimal amount)
    {
        // Processing fee: (amount × rate) + fixed fee
        var fee = (amount * DefaultProcessingFeeRate) + DefaultProcessingFeeFixed;
        return Math.Round(fee, 2);
    }
}
