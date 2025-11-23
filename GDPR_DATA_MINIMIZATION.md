# GDPR Data Minimization - Buyer-Seller Information Sharing

## Overview

This document describes how Mercato minimizes the sharing of buyer personal data with sellers, in accordance with GDPR Article 5(1)(c) (data minimization principle).

## Principles

1. **Need-to-Know Basis:** Sellers receive only the minimum information necessary to fulfill orders
2. **No User Account Access:** Sellers never receive buyer user IDs, passwords, or account metadata
3. **No Payment Details:** Sellers never see buyer payment information (credit card numbers, etc.)
4. **Isolated Order View:** Sellers see only their own SubOrders, not the buyer's full marketplace order

## What Sellers Receive

When a buyer places an order, sellers receive the following information **only for their SubOrder**:

### Essential Fulfillment Information

| Field | Purpose | GDPR Basis |
|-------|---------|------------|
| Delivery Recipient Name | Shipping label | Contract performance (Art. 6(1)(b)) |
| Delivery Address | Shipping destination | Contract performance (Art. 6(1)(b)) |
| Contact Email | Delivery updates/issues | Contract performance (Art. 6(1)(b)) |
| Contact Phone | Delivery coordination | Contract performance (Art. 6(1)(b)) |
| Order Items | Know what to ship | Contract performance (Art. 6(1)(b)) |
| Shipping Method | How to ship | Contract performance (Art. 6(1)(b)) |

### What Sellers DO NOT Receive

- ❌ Buyer User ID
- ❌ Buyer username or login credentials
- ❌ Buyer password (obviously)
- ❌ Buyer payment information (credit card, bank details)
- ❌ Buyer's other orders from different sellers
- ❌ Buyer email marketing preferences
- ❌ Buyer account creation date
- ❌ Buyer's full order history
- ❌ Buyer IP address
- ❌ Buyer's external authentication provider (Google, Facebook)
- ❌ Total marketplace order amount (only their SubOrder total)

## Technical Implementation

### Order Structure

```
Order (Marketplace-wide)
├── OrderId: Guid
├── UserId: string (buyer's user ID - NOT shared with sellers)
├── TotalAmount: decimal (full order total - NOT shared with individual sellers)
├── BuyerEmail: string
├── BuyerPhone: string
├── DeliveryAddress: {...}
└── SubOrders: List<SubOrder>
    ├── SubOrder 1 (Seller A)
    │   ├── SubOrderId: Guid
    │   ├── StoreId: Guid (Seller A)
    │   ├── ProductsTotal: decimal (only Seller A's portion)
    │   ├── ShippingCost: decimal
    │   ├── DeliveryRecipientName: string (for shipping label)
    │   ├── DeliveryAddress: {...} (for shipping)
    │   ├── BuyerEmail: string (for delivery updates)
    │   ├── BuyerPhone: string (for delivery issues)
    │   └── Items: [...] (only Seller A's products)
    │
    └── SubOrder 2 (Seller B)
        ├── ... (separate, isolated from Seller A)
```

### API Endpoints

#### Seller Order Retrieval
**Endpoint:** `GET /api/seller/orders/{subOrderId}`

**Response (SubOrderDto):**
```json
{
  "id": "guid",
  "subOrderNumber": "SUB-2024-000456",
  "storeId": "guid",
  "storeName": "My Store",
  "productsTotal": 45.00,
  "shippingCost": 5.00,
  "totalAmount": 50.00,
  "status": "Pending",
  "deliveryRecipientName": "John Doe",
  "deliveryAddressLine1": "123 Main St",
  "deliveryAddressLine2": "Apt 4B",
  "deliveryCity": "New York",
  "deliveryState": "NY",
  "deliveryPostalCode": "10001",
  "deliveryCountry": "United States",
  "buyerEmail": "buyer@example.com",
  "buyerPhone": "+1-555-0123",
  "items": [
    {
      "productId": "guid",
      "productTitle": "Product Name",
      "quantity": 2,
      "unitPrice": 22.50,
      "subtotal": 45.00
    }
  ],
  "createdAt": "2024-11-23T10:00:00Z"
}
```

**Note:** This response contains **only** the seller's SubOrder. The buyer's `UserId` and other marketplace-level information are **not included**.

## Data Flow Example

### Scenario: Buyer places order with 2 sellers

**Buyer Action:**
1. Buyer adds products from Seller A and Seller B to cart
2. Buyer proceeds to checkout
3. Buyer enters delivery address and payment information
4. Platform creates one Order with two SubOrders

**Platform Processing:**
```
Order (Platform View)
├── OrderId: 550e8400-e29b-41d4-a716-446655440000
├── UserId: "user_12345" (buyer's account)
├── TotalAmount: $100.00
├── SubOrders:
│   ├── SubOrder A (for Seller A) - $60.00
│   └── SubOrder B (for Seller B) - $40.00
```

**Seller A Receives:**
```json
{
  "subOrderId": "...",
  "subOrderNumber": "SUB-2024-000456-A",
  "storeId": "seller-a-store-id",
  "totalAmount": 60.00,
  "deliveryRecipientName": "John Doe",
  "deliveryAddress": "123 Main St, Apt 4B, New York, NY 10001",
  "buyerEmail": "buyer@example.com",
  "buyerPhone": "+1-555-0123",
  "items": [/* only Seller A's products */]
}
```

**Seller B Receives:**
```json
{
  "subOrderId": "...",
  "subOrderNumber": "SUB-2024-000456-B",
  "storeId": "seller-b-store-id",
  "totalAmount": 40.00,
  "deliveryRecipientName": "John Doe",
  "deliveryAddress": "123 Main St, Apt 4B, New York, NY 10001",
  "buyerEmail": "buyer@example.com",
  "buyerPhone": "+1-555-0123",
  "items": [/* only Seller B's products */]
}
```

**What Sellers DO NOT See:**
- Buyer's user ID: `"user_12345"` ❌
- Buyer's full order total: `$100.00` ❌
- Other seller's SubOrder (Seller A doesn't see Seller B's items and vice versa) ❌
- Buyer's payment method or card details ❌

## Payment Information Protection

### Payment Processing Flow

1. **Buyer Checkout:**
   - Buyer enters payment information on platform checkout page
   - Platform securely processes payment through payment gateway
   - Payment transaction ID is recorded

2. **Seller Notification:**
   - Seller receives notification that order is paid
   - Seller **does not** receive:
     - Credit card number (even partial)
     - CVV
     - Cardholder name (unless same as recipient name)
     - Billing address (unless same as delivery address)

3. **Payout to Seller:**
   - Platform holds funds until delivery confirmation
   - Platform transfers seller's portion (minus commission) to seller's bank account
   - Seller receives payout report with:
     - SubOrder numbers fulfilled
     - Amounts earned (net of commission)
     - Payout transaction ID
   - Seller **does not** receive buyer's payment information

## Contact Information Usage Restrictions

### Purpose Limitation

Buyer contact information (email, phone) provided to sellers is restricted to:

✅ **Permitted Uses:**
- Coordinating delivery times
- Notifying about shipment delays
- Resolving delivery issues (e.g., incorrect address)
- Requesting delivery access codes for secure buildings
- Confirming delivery completion

❌ **Prohibited Uses:**
- Marketing or promotional communications
- Sharing with third parties
- Building seller's own customer database
- Cross-selling other products outside the platform
- Adding to email lists without explicit buyer consent

**Note:** Sellers who misuse buyer contact information may face account suspension or termination.

## Compliance Verification

### Seller Agreements

All sellers must agree to:
1. Use buyer information only for order fulfillment
2. Not share buyer information with third parties
3. Not use buyer information for marketing without separate consent
4. Delete buyer information after order completion (or as required by law)

### Platform Controls

The platform enforces data minimization through:
1. **API-Level Filtering:** Seller API endpoints return only necessary SubOrder data
2. **Role-Based Access Control:** Sellers cannot access buyer account information
3. **Audit Logging:** All seller access to buyer data is logged
4. **Automated Monitoring:** Suspicious patterns (e.g., excessive data access) trigger alerts

## GDPR Compliance

This data minimization approach satisfies:

- **Article 5(1)(c) - Data Minimization:** Only necessary data is shared
- **Article 5(1)(b) - Purpose Limitation:** Data used only for order fulfillment
- **Article 25 - Data Protection by Design:** Minimal data sharing built into system architecture
- **Recital 78 - Appropriate Technical Measures:** API filtering and access controls

## Accountability

### Data Controller Responsibilities

**Platform (Mercato):**
- Primary data controller for buyer data
- Responsible for ensuring sellers only receive necessary information
- Maintains audit logs of all data access

**Sellers:**
- Data processors acting on behalf of the platform
- Must follow platform's data handling instructions
- Subject to Data Processing Agreements (DPAs)

### Data Processing Agreements (DPAs)

All sellers enter into a DPA with the platform, which specifies:
- Scope of data processing (order fulfillment only)
- Security measures required
- Data retention limits
- Prohibition on unauthorized use
- Notification requirements for data breaches

## Related Documentation

- [GDPR Compliance Overview](GDPR_COMPLIANCE.md)
- [Privacy Policy](PRIVACY_POLICY.md) _(TODO)_
- [Seller Terms of Service](SELLER_TERMS.md) _(TODO)_
- [Data Processing Agreement](DPA_TEMPLATE.md) _(TODO)_

## Changelog

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2025-11-23 | 1.0 | Initial documentation | GitHub Copilot |

---

**Last Updated:** 2025-11-23  
**Document Owner:** Privacy Officer / DPO  
**Review Cycle:** Annually or when seller access patterns change
