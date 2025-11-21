# Payment Architecture & Flow Documentation

## Overview

The Mercato payment system implements an **escrow-style marketplace payment model** where:

1. Buyers pay the **platform** (Mercato), not sellers directly
2. Funds are held in **escrow** until order delivery is confirmed
3. Platform **deducts commission and fees** before paying out sellers
4. Sellers receive **net amounts** through scheduled payouts

This document describes the payment architecture, flow, commission calculation, and integration points.

---

## Architecture Components

### 1. Payment Gateway Abstraction

**Interface:** `IPaymentGateway`

The payment system uses an abstraction layer to allow swapping payment providers (Stripe, PayU, etc.) without changing core business logic.

**Key Methods:**
- `CreatePaymentSessionAsync()` - Initiates checkout with payment gateway
- `GetPaymentStatusAsync()` - Queries payment status from gateway
- `ValidateWebhookSignature()` - Verifies webhook authenticity
- `RefundPaymentAsync()` - Processes refunds
- `CancelPaymentSessionAsync()` - Cancels pending payments

**Current Implementation:** `MockPaymentGateway` (for development/testing)

**Production TODO:**
- Replace MockPaymentGateway with actual provider (e.g., Stripe, PayU)
- Implement webhook signature validation
- Configure gateway credentials via environment variables
- Test with real payment methods

---

### 2. Domain Models

#### **PaymentTransaction**
Records all payment attempts and outcomes for audit and reconciliation.

**Key Fields:**
- `OrderId` - Links to the Order being paid
- `Amount` - Total amount charged to buyer
- `ProcessingFee` - Gateway fee (calculated as: Amount × 2.9% + $0.30)
- `PaymentGatewaySessionId` - External session ID from gateway
- `PaymentGatewayTransactionId` - External transaction ID from gateway
- `Status` - Pending | Completed | Failed | Refunded | Cancelled
- `ErrorMessage` / `ErrorCode` - For failed transactions

**Purpose:**
- Immutable audit trail of all payment events
- Links orders to external payment gateway transactions
- Enables payment reconciliation and reporting

#### **SubOrderPayment**
Tracks payment breakdown and commission for each SubOrder (seller portion).

**Key Fields:**
- `SubOrderId` - Links to SubOrder in History module
- `PaymentTransactionId` - Links to the main payment
- `StoreId` - Seller receiving this portion
- `ProductTotal` - Base for commission calculation
- `ShippingCost` - NOT subject to commission
- `CommissionRate` - Rate applied (e.g., 0.15 for 15%)
- `CommissionAmount` - ProductTotal × CommissionRate
- `ProcessingFeeAllocated` - Proportional share of gateway fee
- `SellerNetAmount` - What seller receives after deductions
- `PayoutStatus` - Pending (escrow) | Released (deliverable) | PaidOut

**Purpose:**
- Calculates seller's net amount per order
- Tracks escrow status (pending → released → paid out)
- Links SubOrders to payouts
- Provides detailed commission breakdown for transparency

#### **SellerBalance**
Tracks cumulative financial state for each seller/store.

**Key Fields:**
- `StoreId` - Seller's store
- `PendingAmount` - Funds in escrow (awaiting delivery)
- `AvailableAmount` - Released funds ready for payout
- `TotalPaidOut` - Running total of all payouts

**Purpose:**
- Real-time seller balance tracking
- Determines available funds for payout
- Enables seller financial dashboard

#### **Payout**
Records transfers from platform to seller.

**Key Fields:**
- `StoreId` - Seller receiving payout
- `Amount` - Net amount transferred
- `GrossAmount` - Total before deductions
- `CommissionAmount` - Total commission deducted
- `ProcessingFeeAmount` - Total processing fees deducted
- `SubOrderIds` - Comma-separated list of included SubOrders
- `Status` - Pending | Processing | Completed | Failed
- `ScheduledAt` - When payout was created

**Purpose:**
- Payout batch tracking
- Links multiple SubOrders to a single bank transfer
- Audit trail for financial reconciliation

---

## Payment Flow

### 1. Checkout & Payment Initiation

```
┌─────────┐         ┌──────────┐         ┌─────────┐         ┌─────────┐
│  Buyer  │         │   API    │         │ Payment │         │ Gateway │
│         │         │          │         │ Service │         │         │
└────┬────┘         └────┬─────┘         └────┬────┘         └────┬────┘
     │                   │                    │                   │
     │ 1. Checkout       │                    │                   │
     │──────────────────>│                    │                   │
     │                   │                    │                   │
     │                   │ 2. Create Order    │                   │
     │                   │    (Status: Pending)                   │
     │                   │                    │                   │
     │                   │ 3. Initiate Payment│                   │
     │                   │───────────────────>│                   │
     │                   │                    │                   │
     │                   │                    │ 4. Calculate Fee  │
     │                   │                    │                   │
     │                   │                    │ 5. Create PaymentTransaction
     │                   │                    │    (Status: Pending)
     │                   │                    │                   │
     │                   │                    │ 6. Create Session │
     │                   │                    │──────────────────>│
     │                   │                    │                   │
     │                   │                    │ 7. Session Created│
     │                   │                    │<──────────────────│
     │                   │                    │                   │
     │                   │ 8. Checkout URL    │                   │
     │                   │<───────────────────│                   │
     │                   │                    │                   │
     │ 9. Redirect       │                    │                   │
     │<──────────────────│                    │                   │
     │                   │                    │                   │
     │ 10. Complete Payment                   │                   │
     │────────────────────────────────────────────────────────────>│
     │                   │                    │                   │
```

**Steps:**
1. Buyer proceeds to checkout with items in cart
2. API creates Order (Status: Pending, PaymentStatus: Pending)
3. API calls PaymentService.InitiatePaymentAsync()
4. PaymentService calculates processing fee
5. PaymentService creates PaymentTransaction record (Status: Pending)
6. PaymentService calls PaymentGateway.CreatePaymentSessionAsync()
7. Gateway returns session ID and checkout URL
8. PaymentService updates transaction with session ID
9. API redirects buyer to gateway checkout URL
10. Buyer completes payment on gateway

---

### 2. Payment Confirmation

```
┌─────────┐         ┌──────────┐         ┌─────────┐         ┌─────────┐
│ Gateway │         │   API    │         │ Payment │         │  Order  │
│         │         │          │         │ Service │         │ Service │
└────┬────┘         └────┬─────┘         └────┬────┘         └────┬────┘
     │                   │                    │                   │
     │ 1. Webhook/Callback                    │                   │
     │──────────────────>│                    │                   │
     │                   │                    │                   │
     │                   │ 2. Confirm Payment │                   │
     │                   │───────────────────>│                   │
     │                   │                    │                   │
     │                   │                    │ 3. Update PaymentTransaction
     │                   │                    │    (Status: Completed)
     │                   │                    │                   │
     │                   │                    │ 4. Record SubOrderPayments
     │                   │                    │    (with commission calc)
     │                   │                    │                   │
     │                   │                    │ 5. Update SellerBalances
     │                   │                    │    (add to PendingAmount)
     │                   │                    │                   │
     │                   │                    │ 6. Update Order   │
     │                   │                    │──────────────────>│
     │                   │                    │                   │
     │                   │                    │                   │ 7. Update Order
     │                   │                    │                   │    (PaymentStatus: Paid)
     │                   │                    │                   │
     │                   │                    │                   │ 8. Notify Sellers
     │                   │                    │                   │
     │                   │ 9. Success Response│                   │
     │                   │<───────────────────│                   │
     │                   │                    │                   │
     │ 10. Acknowledge   │                    │                   │
     │<──────────────────│                    │                   │
     │                   │                    │                   │
```

**Steps:**
1. Payment gateway sends webhook or buyer is redirected with status
2. API calls PaymentService.ConfirmPaymentAsync()
3. PaymentService updates PaymentTransaction status to Completed
4. PaymentService creates SubOrderPayment records with commission calculations
5. PaymentService updates SellerBalance (adds to PendingAmount)
6. PaymentService notifies OrderService to update order
7. OrderService updates Order.PaymentStatus to "Paid"
8. OrderService notifies sellers of new orders
9. PaymentService returns success
10. API acknowledges webhook to gateway

---

### 3. Escrow Release (After Delivery)

```
┌─────────┐         ┌──────────┐         ┌─────────┐
│ Seller  │         │  Order   │         │ Payment │
│         │         │ Service  │         │ Service │
└────┬────┘         └────┬─────┘         └────┬────┘
     │                   │                    │
     │ 1. Mark Shipped   │                    │
     │──────────────────>│                    │
     │                   │                    │
     │                   │ (wait 14 days or   │
     │                   │  buyer confirms)   │
     │                   │                    │
     │                   │ 2. SubOrder Delivered
     │                   │                    │
     │                   │ 3. Release Escrow  │
     │                   │───────────────────>│
     │                   │                    │
     │                   │                    │ 4. Update SubOrderPayment
     │                   │                    │    (Status: Released)
     │                   │                    │
     │                   │                    │ 5. Update SellerBalance
     │                   │                    │    PendingAmount → AvailableAmount
     │                   │                    │
     │                   │ 6. Escrow Released │
     │                   │<───────────────────│
     │                   │                    │
```

**Steps:**
1. Seller marks SubOrder as Shipped
2. After 14 days or manual buyer confirmation, SubOrder status → Delivered
3. OrderService calls PaymentService.ReleaseEscrowForSubOrderAsync()
4. PaymentService updates SubOrderPayment.PayoutStatus to "Released"
5. PaymentService moves funds from PendingAmount to AvailableAmount
6. Funds are now available for payout

---

### 4. Payout Processing

```
┌─────────┐         ┌──────────┐         ┌─────────┐
│  Admin  │         │ Payment  │         │  Bank   │
│  /Cron  │         │ Service  │         │ System  │
└────┬────┘         └────┬─────┘         └────┬────┘
     │                   │                    │
     │ 1. Trigger Payout │                    │
     │──────────────────>│                    │
     │                   │                    │
     │                   │ 2. Check Available Balance
     │                   │                    │
     │                   │ 3. Get Released SubOrderPayments
     │                   │                    │
     │                   │ 4. Create Payout Record
     │                   │    (Status: Pending)
     │                   │                    │
     │                   │ 5. Update SubOrderPayments
     │                   │    (Status: PaidOut)
     │                   │                    │
     │                   │ 6. Update SellerBalance
     │                   │    AvailableAmount → TotalPaidOut
     │                   │                    │
     │                   │ 7. Initiate Transfer
     │                   │───────────────────>│
     │                   │                    │
     │                   │ 8. Transfer Complete│
     │                   │<───────────────────│
     │                   │                    │
     │                   │ 9. Update Payout   │
     │                   │    (Status: Completed)
     │                   │                    │
     │ 10. Payout Done   │                    │
     │<──────────────────│                    │
     │                   │                    │
```

**Steps:**
1. Admin triggers payout or scheduled job runs (weekly on Mondays)
2. PaymentService checks SellerBalance.AvailableAmount
3. PaymentService retrieves all Released SubOrderPayments for the store
4. PaymentService creates Payout record
5. PaymentService marks SubOrderPayments as PaidOut
6. PaymentService updates SellerBalance (available → paid out)
7. PaymentService initiates bank transfer (future: via gateway API)
8. Bank confirms transfer completion
9. PaymentService updates Payout status to Completed
10. Seller receives funds in bank account

---

## Commission Calculation

### Formula

For each SubOrder:

```
ProductTotal = Sum of (Item.Price × Item.Quantity)
ShippingCost = Seller's shipping fee

SubOrderTotal = ProductTotal + ShippingCost

CommissionAmount = ProductTotal × CommissionRate
  (Note: Commission is NOT applied to shipping)

ProcessingFeeAllocated = (SubOrderTotal / OrderTotal) × TotalProcessingFee
  (Processing fee is distributed proportionally across SubOrders)

SellerNetAmount = SubOrderTotal - CommissionAmount - ProcessingFeeAllocated
```

### Configuration

- **Default Commission Rate:** 15% (0.15)
- **Processing Fee Rate:** 2.9% + $0.30 per transaction
- **Commission Base:** Product total only (excludes shipping)

### Example Calculation

**Scenario:** Order with 2 sellers

| Field | Seller A | Seller B | Total |
|-------|----------|----------|-------|
| Product Total | $80.00 | $20.00 | $100.00 |
| Shipping Cost | $8.00 | $2.00 | $10.00 |
| **SubOrder Total** | **$88.00** | **$22.00** | **$110.00** |

**Total Processing Fee:** ($110 × 0.029) + $0.30 = $3.49

**Seller A Breakdown:**
- Commission: $80 × 0.15 = $12.00
- Processing Fee: ($88 / $110) × $3.49 = $2.79
- Net Amount: $88 - $12.00 - $2.79 = **$73.21**

**Seller B Breakdown:**
- Commission: $20 × 0.15 = $3.00
- Processing Fee: ($22 / $110) × $3.49 = $0.70
- Net Amount: $22 - $3.00 - $0.70 = **$18.30**

**Platform Revenue:**
- Commission: $12.00 + $3.00 = $15.00
- Processing Fee Coverage: $3.49 (to gateway)
- Net Revenue: $15.00

**Verification:** $73.21 + $18.30 + $15.00 + $3.49 = $110.00 ✓

---

## Error Handling

### Payment Failures

**Scenario:** Gateway declines payment

**Flow:**
1. Gateway returns failure status via webhook
2. PaymentService updates PaymentTransaction status to "Failed"
3. PaymentService records error message and code
4. Order status remains "Pending"
5. Buyer can retry payment or cancel order

**Key Points:**
- Cart items remain reserved during retry window
- Stock is NOT deducted until payment succeeds
- Failed transactions are logged for audit

### Payment Cancellations

**Scenario:** Buyer cancels during payment

**Flow:**
1. Buyer clicks "Cancel" on gateway page
2. Gateway redirects to cancel URL
3. PaymentService updates PaymentTransaction status to "Cancelled"
4. Order can be deleted or remains as "Pending"
5. Buyer returns to cart to modify order

### Webhook Failures

**Scenario:** Webhook delivery fails

**Mitigation:**
- Gateway retries webhook delivery (typically 3-5 attempts)
- Payment status polling as fallback (query gateway API)
- Manual reconciliation for edge cases
- Alert system for failed webhooks

**TODO:** Implement webhook retry and fallback polling

---

## Security Considerations

### 1. Webhook Signature Validation

**CRITICAL:** Webhooks must be authenticated before processing.

**Current State:** MockPaymentGateway does NOT validate signatures (development only)

**Production Requirements:**
```csharp
// Example: Stripe webhook validation
var signature = Request.Headers["Stripe-Signature"];
var payload = await new StreamReader(Request.Body).ReadToEndAsync();
var secret = Configuration["Stripe:WebhookSecret"];

if (!_paymentGateway.ValidateWebhookSignature(payload, signature, secret))
{
    return Unauthorized();
}
```

**Action Items:**
- Implement signature validation per gateway documentation
- Store webhook secrets securely (Azure Key Vault, AWS Secrets Manager)
- Log all webhook validation failures
- Test signature validation with gateway webhooks

### 2. Authorization

**Current Implementation:**
- Payment initiation requires authenticated user
- Balance queries require Seller or Admin role
- Payout creation requires Seller or Admin role
- Escrow release restricted to Admin role

**TODO:**
- Verify user owns the order being paid
- Verify seller owns the store for balance/payout queries
- Add rate limiting to prevent abuse

### 3. Data Integrity

**Measures:**
- PaymentTransaction is immutable (no updates after completion)
- SubOrderPayment tracks commission calculations for audit
- All financial amounts use decimal(18,2) precision
- Database transactions ensure atomicity

---

## Integration Points

### With Order Management (History Module)

**Required:**
1. OrderService must call PaymentService.InitiatePaymentAsync() after order creation
2. OrderService must update Order.PaymentStatus when payment confirmed
3. OrderService must call PaymentService.ReleaseEscrowForSubOrderAsync() when SubOrder delivered

**Data Flow:**
- Order → PaymentTransaction (via OrderId)
- SubOrder → SubOrderPayment (via SubOrderId)

**TODO:** Implement these integrations in OrderService

### With Seller Panel Module

**Required:**
1. Store ownership verification for balance queries
2. Bank account details for payouts
3. Commission rate configuration per store (default: 15%)

**TODO:** Add methods to verify store ownership

### With Notification Module

**Required:**
1. Notify buyer of payment success/failure
2. Notify sellers when order is paid
3. Notify sellers when payout is initiated/completed

**TODO:** Implement notification hooks

---

## Configuration

### appsettings.json

```json
{
  "Payment": {
    "Gateway": "Mock", // Mock | Stripe | PayU
    "ProcessingFeeRate": 0.029,
    "ProcessingFeeFixed": 0.30,
    "DefaultCommissionRate": 0.15,
    "MinimumPayout": 10.00,
    "PayoutSchedule": "Weekly",
    "PayoutDay": "Monday"
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "PayU": {
    "MerchantId": "...",
    "ApiKey": "...",
    "WebhookSecret": "..."
  }
}
```

### Environment Variables (Production)

```bash
# Never commit these to source control
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
PAYU_API_KEY=...
```

---

## Future Enhancements

### Phase 2 (Post-MVP)

1. **Multiple Payment Methods**
   - Credit/Debit cards ✓
   - Bank transfers
   - Digital wallets (Apple Pay, Google Pay)
   - BLIK (Poland-specific)

2. **Refunds**
   - Full refunds for cancelled orders
   - Partial refunds for returned items
   - Refund fee handling

3. **Split Payments**
   - Pay with multiple methods
   - Gift cards and store credit

4. **Recurring Payments**
   - Subscription products
   - Installment plans

5. **Multi-Currency**
   - Dynamic currency conversion
   - Local payment methods per region

### Advanced Features

- **Fraud Detection:** Integrate with gateway fraud tools
- **Chargeback Management:** Handle disputed transactions
- **Automated Reconciliation:** Daily payment reports and matching
- **Payout Automation:** Scheduled batch processing
- **Tax Calculation:** VAT/sales tax integration
- **Financial Reporting:** Revenue, commission, and payout dashboards

---

## Testing Strategy

### Unit Tests

**PaymentService:**
- Commission calculation accuracy
- Escrow state transitions
- Balance updates
- Error handling

**MockPaymentGateway:**
- Session creation
- Status queries
- Cancellation flow

### Integration Tests

**End-to-End Flow:**
1. Create order → Initiate payment → Confirm payment
2. Verify PaymentTransaction created
3. Verify SubOrderPayments created with correct commission
4. Verify SellerBalance updated (pending amount)

**Escrow Release:**
1. Mark SubOrder delivered
2. Verify escrow released
3. Verify balance moved to available

**Payout:**
1. Create payout
2. Verify SubOrderPayments marked as paid out
3. Verify balance updated

### Manual Testing (Before Production)

1. Complete purchase with real gateway (test mode)
2. Test webhook delivery and handling
3. Verify commission calculations match business rules
4. Test failure and cancellation scenarios
5. Verify payout creation and tracking

---

## Monitoring & Observability

### Key Metrics

- Payment success rate (target: >98%)
- Average payment processing time
- Failed payment reasons (categorized)
- Escrow release lag (SubOrder delivery → escrow release)
- Payout completion rate
- Seller balance accuracy

### Alerts

- Payment failure rate exceeds 5%
- Webhook delivery failures
- Payout failures
- Balance reconciliation mismatches
- Gateway downtime

### Logging

**Critical Events:**
- Payment initiated (with order and amount)
- Payment completed/failed (with status and reason)
- Escrow released (with SubOrder and amount)
- Payout created/completed (with store and amount)

**Log Level Guidelines:**
- Info: Normal flow (initiation, confirmation, release, payout)
- Warning: Retryable failures (webhook retry, gateway timeout)
- Error: Non-retryable failures (validation errors, balance insufficient)
- Critical: Data integrity issues (reconciliation mismatch)

---

## Deployment Checklist

### Before Production

- [ ] Replace MockPaymentGateway with real gateway implementation
- [ ] Implement webhook signature validation
- [ ] Configure gateway credentials via environment variables
- [ ] Test real payment flow in gateway test mode
- [ ] Set up webhook endpoints with HTTPS
- [ ] Enable SSL/TLS for all payment endpoints
- [ ] Configure CORS for allowed frontend domains
- [ ] Set up monitoring and alerting
- [ ] Create runbook for payment failures
- [ ] Train support team on payment troubleshooting
- [ ] Review and test error handling paths
- [ ] Verify commission calculations with accounting team
- [ ] Set up backup payment gateway (redundancy)

### Database Migrations

```bash
# Apply migrations
dotnet ef database update --context PaymentsDbContext

# Verify tables created:
# - PaymentTransactions
# - SellerBalances
# - Payouts
# - SubOrderPayments
```

---

## Support & Troubleshooting

### Common Issues

**Issue:** Payment appears successful but order not confirmed

**Diagnosis:**
1. Check PaymentTransaction status
2. Verify webhook was received and processed
3. Check for errors in logs during ConfirmPaymentAsync
4. Manually trigger payment confirmation if needed

**Issue:** Seller balance doesn't match expected amount

**Diagnosis:**
1. Query SubOrderPayments for the store
2. Sum SellerNetAmount for Released and PaidOut SubOrders
3. Compare with SellerBalance.AvailableAmount + TotalPaidOut
4. Check for missing escrow release calls

**Issue:** Payout fails

**Diagnosis:**
1. Check SellerBalance.AvailableAmount >= payout amount
2. Verify bank account details in store profile
3. Check gateway API logs for transfer errors
4. Retry payout or create new one

---

## Contact & Resources

**Payment Gateway Documentation:**
- Stripe: https://stripe.com/docs/api
- PayU: https://developers.payu.com/

**Internal Contacts:**
- Payment Issues: payment-support@mercato.com
- Financial Reconciliation: finance@mercato.com
- Technical Issues: dev-team@mercato.com

---

**Document Version:** 1.0  
**Last Updated:** 2025-11-21  
**Author:** AI Development Team  
**Status:** Initial implementation complete, pending integration testing
