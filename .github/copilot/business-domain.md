# Mercato – Business Domain Documentation

This document describes the core business domain of the Mercato multi-vendor marketplace platform. It serves as the primary reference for understanding business rules, entities, processes, and terminology for the MVP implementation.

---

## 1. Overview

Mercato is a multi-vendor marketplace that connects independent online stores in a unified platform. The system enables sellers to manage their stores and products while buyers can browse across all sellers, make purchases, and track their orders.

### 1.1 Core Value Proposition

- **For Sellers**: A ready-to-use e-commerce platform with built-in customer base, payment processing, and order management.
- **For Buyers**: Access to multiple stores in one place with unified search, cart, and checkout experience.
- **For Platform**: Revenue through commission-based fees on transactions.

---

## 2. MVP Scope

The MVP focuses on delivering the complete end-to-end purchase process:

1. **Seller Onboarding & Product Listing**
   - Sellers can register and create store profiles
   - Sellers can add and manage products
   - Products are visible in the global catalog

2. **Full Checkout & Online Payment**
   - Buyers can browse products across all sellers
   - Buyers can add items to cart from multiple sellers
   - Complete checkout process with online payment
   - Payment escrow model (see section 4)

3. **Order & Delivery Management**
   - Order creation and tracking
   - Seller fulfillment workflow
   - Delivery status tracking
   - Seller payout processing

---

## 3. Core Business Flows

### 3.1 Seller Onboarding Flow

**Actors**: New Seller, Platform Administrator

**Pre-conditions**: User has a valid email address

**Flow**:
1. Seller registers as a new user
2. Seller completes store profile:
   - Store name (required, unique across platform)
   - Store description
   - Contact information (email, phone)
   - Business details (company name, tax ID)
   - Bank account for payouts
3. Seller agrees to platform terms and commission rates
4. Platform validates seller information
5. Store is activated and visible on the platform

**Post-conditions**: 
- Seller account is created with "Seller" role
- Store profile is active
- Seller can access Seller Panel
- Store appears in seller directory

**Business Rules**:
- Store names must be unique across the platform
- Email must be verified before store activation
- Tax ID must be valid (format validation only in MVP)
- Minimum age requirement: 18 years
- Default commission rate: 15% (configurable by admin)

// TODO: Should sellers be auto-approved or require admin approval? Current assumption: auto-approved for MVP.
// TODO: What are the minimum required fields for bank account details (will vary by country)?

---

### 3.2 Product Listing Flow

**Actors**: Seller

**Pre-conditions**: Seller has an active store

**Flow**:
1. Seller creates a new product:
   - Title (required)
   - Description (required)
   - Category (required, from predefined list)
   - SKU (required, unique within seller's store)
   - Price (required, positive decimal)
   - Stock quantity (required, non-negative integer)
   - Product images (at least one required)
   - Weight and dimensions (for shipping calculation)
2. Seller sets product visibility (Draft/Published)
3. System validates product data
4. Product is saved and indexed for search

**Post-conditions**:
- Product is created in seller's catalog
- If published, product appears in global catalog
- Product is searchable by buyers

**Business Rules**:
- SKU must be unique within the seller's store
- Price must be positive (> 0)
- Stock quantity cannot be negative
- At least one product image is required
- Product title max length: 200 characters
- Description max length: 5000 characters
- Maximum 10 images per product
- Supported image formats: JPEG, PNG, WebP
- Max image size: 5 MB per image

// TODO: Should there be a minimum price requirement to prevent $0.01 listings?
// TODO: Are there restricted categories or products that require approval?

---

### 3.3 Product Browsing and Search Flow

**Actors**: Buyer (authenticated or guest)

**Pre-conditions**: None

**Flow**:
1. Buyer accesses the marketplace
2. Buyer can:
   - Browse featured products
   - Browse by category
   - Search by keywords
   - Filter by price range, seller, availability
   - Sort by relevance, price, newest
3. Buyer views product details including:
   - Product information
   - Seller information
   - Stock availability
   - Shipping options
   - Customer reviews (future phase)

**Post-conditions**:
- Buyer sees relevant products
- Products are displayed with current availability

**Business Rules**:
- Only published products with stock > 0 are visible
- Search results are ranked by relevance
- Products from deactivated sellers are hidden
- Out-of-stock products can be viewed but not added to cart

---

### 3.4 Shopping Cart Flow

**Actors**: Buyer

**Pre-conditions**: Buyer is authenticated (guest checkout in future phase)

**Flow**:
1. Buyer adds product to cart:
   - Selects quantity (must be ≤ available stock)
   - Product is added to cart
2. Cart automatically groups items by seller
3. Buyer can:
   - Update quantities
   - Remove items
   - View cart summary with:
     - Subtotal per seller
     - Shipping costs per seller
     - Total amount
4. Cart persists across sessions

**Post-conditions**:
- Cart contains selected items
- Cart shows accurate pricing and availability
- Cart is ready for checkout

**Business Rules**:
- Cart item quantity cannot exceed available stock
- Cart item quantity must be positive integer
- If product price changes, cart shows updated price
- If product becomes unavailable, buyer is notified
- Cart expires after 30 days of inactivity
- Maximum 50 unique items per cart

// TODO: How should we handle stock changes while items are in cart? Lock stock or notify at checkout?
// TODO: Should there be a maximum order value limit for MVP?

---

### 3.5 Checkout and Payment Flow

**Actors**: Buyer, Payment Gateway, Platform, Seller

**Pre-conditions**: 
- Buyer is authenticated
- Cart contains valid items
- Selected shipping methods are available

**Flow**:
1. **Checkout Initiation**:
   - Buyer reviews cart
   - Buyer provides/confirms:
     - Shipping address
     - Billing address
     - Contact information

2. **Shipping Selection**:
   - For each seller in cart, buyer selects shipping method
   - System calculates shipping costs
   - System displays total order amount

3. **Payment**:
   - Buyer selects payment method
   - Buyer is redirected to payment gateway
   - Buyer completes payment
   - Payment gateway processes payment
   - Payment gateway notifies platform of result

4. **Order Creation**:
   - If payment successful:
     - Platform creates main Order
     - Platform creates SubOrders for each seller
     - Platform deducts stock quantities
     - Platform sends confirmation to buyer
     - Platform notifies each seller of new order
   - If payment failed:
     - Cart items remain in cart
     - Buyer is notified of failure
     - Buyer can retry payment

**Post-conditions**:
- Order is created with "Paid" status
- SubOrders are created for each seller with "Processing" status
- Payment transaction is recorded
- Stock is reserved/deducted
- Buyer receives order confirmation
- Sellers receive order notifications

**Business Rules**:
- Payment must be successful before order creation
- All cart validations must pass before payment:
  - Products still available
  - Prices haven't changed significantly (>10%)
  - Stock is sufficient
- Order gets unique marketplace-level order number
- Each SubOrder gets unique sub-order number
- Shipping address must be complete and valid
- Supported payment methods: credit/debit cards (via gateway)

// TODO: What is the acceptable price change threshold? 10% seems reasonable, but needs business confirmation.
// TODO: Should we support partial fulfillment if some items become unavailable during checkout?

---

### 3.6 Payment Escrow and Distribution

**Actors**: Platform, Seller, Payment Gateway

**Payment Model**: Central escrow-like system

**Flow**:
1. **Payment Collection**:
   - Buyer pays total order amount to Mercato platform
   - Payment is held in platform account (escrow)
   - Order status: "Paid"

2. **Order Fulfillment**:
   - Seller processes SubOrder
   - Seller ships items
   - Seller marks SubOrder as "Shipped"
   - SubOrder status: "Shipped"

3. **Delivery Confirmation**:
   - Buyer receives items
   - Delivery is confirmed (automatic after X days or manual by buyer)
   - SubOrder status: "Delivered"

4. **Payout Calculation**:
   - Platform calculates seller payout:
     - SubOrder total (products + shipping)
     - Minus platform commission (%)
     - Minus payment processing fees
   - Payout amount = SubOrder Total × (1 - Commission Rate) - Processing Fees

5. **Payout Execution**:
   - Platform initiates payout to seller's bank account
   - Payout status: "Pending" → "Completed"
   - Seller receives funds
   - Transaction is recorded

**Post-conditions**:
- Seller receives net amount
- Platform retains commission
- Payment transaction is fully reconciled

**Business Rules**:
- Payout occurs only after delivery confirmation
- Default auto-confirmation period: 14 days after shipment
- Commission rate: 15% (configurable per seller by admin)
- Payment processing fees: 2.9% + $0.30 per transaction
- Minimum payout amount: $10
- Payout frequency: Weekly (every Monday)
- Seller must have verified bank account

**Payout Formula**:
```
Gross Amount = SubOrder Product Total + Shipping Fee
Payment Processing Fee = (Gross Amount × 0.029) + 0.30
Platform Commission = SubOrder Product Total × Commission Rate
Net Payout = Gross Amount - Platform Commission - Payment Processing Fee
```

**Example Calculation**:
- SubOrder Total: $100 (products) + $10 (shipping) = $110
- Payment Processing Fee: ($110 × 0.029) + $0.30 = $3.49
- Platform Commission: $100 × 0.15 = $15
- Net Payout to Seller: $110 - $15 - $3.49 = $91.51

// TODO: Should platform commission apply to shipping fees? Current assumption: No, only on product total.
// TODO: What happens if buyer initiates return/refund? How are fees handled?

---

### 3.7 Order Management Flow (Seller Side)

**Actors**: Seller

**Pre-conditions**: SubOrder exists and is assigned to seller

**Flow**:
1. Seller receives notification of new order
2. Seller views SubOrder details:
   - Items ordered
   - Quantities
   - Buyer shipping address
   - Payment status
3. Seller processes order:
   - Prepares items for shipment
   - Prints shipping label
   - Ships package
4. Seller updates SubOrder status to "Shipped"
5. Seller enters tracking information (optional but recommended)
6. System notifies buyer of shipment

**Post-conditions**:
- SubOrder status is "Shipped"
- Buyer is notified
- Tracking information is available
- Delivery countdown starts

**Business Rules**:
- Seller must ship within 3 business days of order placement
- Seller must provide tracking number (recommended)
- Seller cannot cancel order after shipping
- Late shipment may affect seller rating (future phase)

// TODO: What are penalties for late shipment? Warning system? Automated seller rating impact?

---

### 3.8 Order Tracking Flow (Buyer Side)

**Actors**: Buyer

**Pre-conditions**: Order exists and is paid

**Flow**:
1. Buyer accesses order history
2. Buyer views order details:
   - Order number
   - Order date
   - SubOrders by seller
   - Current status per SubOrder
   - Tracking information (if provided)
   - Expected delivery date
3. Buyer can:
   - Track shipment (if tracking number provided)
   - Confirm delivery
   - Request return/refund (future phase)

**Post-conditions**:
- Buyer is informed of order status
- Buyer can make informed decisions

**Business Rules**:
- Order history shows last 12 months by default
- SubOrders can have different statuses
- Delivery is auto-confirmed 14 days after shipment if not manually confirmed
- Buyer can confirm delivery manually anytime after shipment

---

## 4. Key Business Entities

### 4.1 User
Represents any authenticated person in the system.

**Attributes**:
- UserID (unique identifier)
- Email (unique, required)
- Password (hashed, required)
- FirstName (required)
- LastName (required)
- PhoneNumber (optional)
- Role (Buyer | Seller | Admin)
- IsEmailVerified (boolean)
- CreatedAt (timestamp)
- LastLoginAt (timestamp)

**Business Rules**:
- Email must be unique across all users
- Password must meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit)
- Email must be verified before account is fully active
- A user can have only one primary role (multi-role support in future)

---

### 4.2 Store (Seller Profile)
Represents a seller's store on the platform.

**Attributes**:
- StoreID (unique identifier)
- OwnerUserID (foreign key to User)
- StoreName (unique, required)
- Description (optional)
- LogoURL (optional)
- ContactEmail (required)
- PhoneNumber (optional)
- BusinessName (required)
- TaxID (required)
- BankAccountDetails (required for payouts)
- CommissionRate (decimal, default 0.15)
- IsActive (boolean)
- CreatedAt (timestamp)

**Business Rules**:
- One user can own only one store (in MVP)
- Store name must be unique across platform
- Store cannot be deleted if it has active orders
- Inactive stores' products are hidden from catalog

---

### 4.3 Product
Represents an item for sale in a seller's store.

**Attributes**:
- ProductID (unique identifier)
- StoreID (foreign key to Store)
- SKU (unique within store, required)
- Title (required, max 200 chars)
- Description (required, max 5000 chars)
- CategoryID (foreign key to Category)
- Price (decimal, required, positive)
- StockQuantity (integer, non-negative)
- Weight (decimal, for shipping)
- Dimensions (length × width × height)
- ImageURLs (array, at least one required)
- Status (Draft | Published | Archived)
- CreatedAt (timestamp)
- UpdatedAt (timestamp)

**Business Rules**:
- SKU must be unique within seller's store
- Price must be > 0
- Stock quantity cannot be negative
- Published products must have at least one image
- Archived products are hidden but not deleted

---

### 4.4 Category
Represents a product category.

**Attributes**:
- CategoryID (unique identifier)
- Name (unique, required)
- ParentCategoryID (nullable, for hierarchy)
- Description (optional)
- IsActive (boolean)

**Business Rules**:
- Category hierarchy max depth: 3 levels
- Category can be deactivated but not deleted if products exist
- Deactivated categories are hidden from new product creation

---

### 4.5 Cart
Represents a buyer's shopping cart.

**Attributes**:
- CartID (unique identifier)
- UserID (foreign key to User)
- CreatedAt (timestamp)
- UpdatedAt (timestamp)
- ExpiresAt (timestamp, 30 days from creation)

**Cart Item** (line item in cart):
- CartItemID (unique identifier)
- CartID (foreign key to Cart)
- ProductID (foreign key to Product)
- Quantity (integer, positive)
- PriceAtAdd (decimal, snapshot of product price when added)

**Business Rules**:
- One active cart per user
- Cart expires after 30 days of inactivity
- Max 50 unique items per cart
- Quantity per item cannot exceed product stock

---

### 4.6 Order
Represents a marketplace-level order (buyer's complete purchase).

**Attributes**:
- OrderID (unique identifier)
- OrderNumber (unique, human-readable, e.g., "MKT-2024-000001")
- UserID (foreign key to User - buyer)
- ShippingAddress (embedded object)
- BillingAddress (embedded object)
- TotalAmount (decimal)
- PaymentStatus (Pending | Paid | Failed | Refunded)
- PaymentTransactionID (reference to payment gateway)
- CreatedAt (timestamp)

**Shipping Address**:
- RecipientName
- AddressLine1
- AddressLine2 (optional)
- City
- State/Province
- PostalCode
- Country
- PhoneNumber

**Business Rules**:
- Order number must be unique and sequential
- Order is created only after successful payment
- Order contains one or more SubOrders

---

### 4.7 SubOrder
Represents seller-specific portion of an order.

**Attributes**:
- SubOrderID (unique identifier)
- SubOrderNumber (unique, human-readable, e.g., "SUB-2024-000001")
- OrderID (foreign key to Order)
- StoreID (foreign key to Store)
- Status (Processing | Shipped | Delivered | Cancelled)
- Subtotal (decimal, sum of items)
- ShippingCost (decimal)
- TotalAmount (decimal)
- TrackingNumber (optional)
- ShippingMethod (string)
- ShippedAt (timestamp, nullable)
- DeliveredAt (timestamp, nullable)
- CreatedAt (timestamp)

**SubOrder Item** (line item in sub-order):
- SubOrderItemID (unique identifier)
- SubOrderID (foreign key to SubOrder)
- ProductID (foreign key to Product)
- Quantity (integer)
- PricePerUnit (decimal, price at order time)
- TotalPrice (decimal, quantity × price)

**Business Rules**:
- SubOrder status transitions: Processing → Shipped → Delivered
- SubOrder cannot be cancelled after shipping
- Delivery is auto-confirmed 14 days after shipment
- One SubOrder per seller per Order

---

### 4.8 Payment Transaction
Represents a payment event.

**Attributes**:
- TransactionID (unique identifier)
- OrderID (foreign key to Order)
- Amount (decimal)
- Currency (string, default "USD")
- PaymentMethod (CreditCard | DebitCard | etc.)
- PaymentGatewayID (external reference)
- Status (Pending | Completed | Failed | Refunded)
- ProcessingFee (decimal)
- CreatedAt (timestamp)
- CompletedAt (timestamp, nullable)

**Business Rules**:
- Transaction amount must match order total
- Only one successful payment per order
- Failed payments are logged for audit

---

### 4.9 Payout
Represents a payment from platform to seller.

**Attributes**:
- PayoutID (unique identifier)
- StoreID (foreign key to Store)
- Amount (decimal, net amount to seller)
- GrossAmount (decimal, before fees)
- CommissionAmount (decimal)
- ProcessingFeeAmount (decimal)
- Status (Pending | Processing | Completed | Failed)
- PayoutMethod (BankTransfer | etc.)
- ScheduledAt (timestamp)
- CompletedAt (timestamp, nullable)
- SubOrderIDs (array of references to included SubOrders)

**Business Rules**:
- Payout includes all delivered SubOrders from previous week
- Minimum payout: $10
- Payout occurs weekly (every Monday)
- Failed payouts are retried next cycle

---

## 5. Business Rules Summary

### 5.1 Product Rules
- ✓ SKU must be unique within seller's store
- ✓ Price must be positive (> 0)
- ✓ Stock quantity cannot be negative
- ✓ At least one image required for published products
- ✓ Maximum 10 images per product
- ✓ Max image size: 5 MB, formats: JPEG, PNG, WebP

### 5.2 Cart Rules
- ✓ One active cart per user
- ✓ Cart expires after 30 days of inactivity
- ✓ Maximum 50 unique items per cart
- ✓ Cart item quantity cannot exceed available stock

### 5.3 Order Rules
- ✓ Order created only after successful payment
- ✓ Stock is deducted upon order creation
- ✓ Order number is unique and sequential
- ✓ One Order can contain multiple SubOrders (one per seller)

### 5.4 SubOrder Rules
- ✓ Seller must ship within 3 business days
- ✓ Status flow: Processing → Shipped → Delivered
- ✓ Delivery auto-confirmed after 14 days
- ✓ Cannot cancel after shipping

### 5.5 Payment Rules
- ✓ Buyer pays platform (escrow model)
- ✓ Platform commission: 15% of product total
- ✓ Payment processing fees: 2.9% + $0.30
- ✓ Payout after delivery confirmation
- ✓ Minimum payout: $10
- ✓ Payout frequency: Weekly

### 5.6 Seller Rules
- ✓ Store name must be unique
- ✓ Email must be verified
- ✓ One store per user (MVP)
- ✓ Bank account required for payouts
- ✓ Commission rate configurable by admin (default 15%)

---

## 6. Business Validations

### 6.1 At Registration
- Email format and uniqueness
- Password complexity
- Age requirement (18+)
- Required fields completeness

### 6.2 At Product Creation
- SKU uniqueness within store
- Price is positive decimal
- Stock is non-negative integer
- Category exists and is active
- At least one image if publishing
- Title and description within limits

### 6.3 At Checkout
- All cart items still available
- Sufficient stock for all items
- Prices haven't changed significantly (>10%)
- Valid shipping address
- Selected shipping methods are available
- Payment method is supported

### 6.4 At Order Fulfillment
- Order is paid
- SubOrder belongs to authenticated seller
- SubOrder is in correct status for transition
- Tracking number format (if provided)

---

## 7. Domain Glossary

| Term | Definition |
|------|------------|
| **Marketplace** | The Mercato platform connecting multiple sellers and buyers |
| **Seller** | A registered user who owns a store and sells products |
| **Buyer** | A user who browses and purchases products |
| **Store** | A seller's virtual shop on the platform |
| **Product** | An item for sale in a store |
| **SKU** | Stock Keeping Unit - unique product identifier within a store |
| **Cart** | Collection of products a buyer intends to purchase |
| **Order** | Marketplace-level purchase by a buyer (can include items from multiple sellers) |
| **SubOrder** | Seller-specific portion of an order |
| **Commission** | Platform fee charged to seller as percentage of product sale |
| **Escrow** | Payment model where platform holds buyer payment until delivery |
| **Payout** | Transfer of funds from platform to seller |
| **Gross Amount** | Total amount before deducting fees |
| **Net Amount** | Amount after deducting commission and fees |
| **Processing Fee** | Payment gateway fee for processing transactions |
| **Fulfillment** | Process of preparing and shipping ordered items |
| **Tracking Number** | Unique identifier for shipped package |
| **Delivery Confirmation** | Acknowledgment that buyer received the package |

---

## 8. MVP Constraints and Assumptions

### 8.1 Constraints
- Single currency (USD)
- Single country/region
- Credit/debit card payments only
- English language only
- Desktop-first UI (mobile responsive)

### 8.2 Assumptions
- Sellers are auto-approved (no manual review)
- Guest checkout not supported (users must register)
- No product variants (size, color) - each variant is separate product
- No promotional codes or discounts
- No product reviews or ratings
- No seller messaging system
- No advanced fraud detection
- No subscription products
- No digital/downloadable products
- Physical products with shipping only

### 8.3 Out of Scope for MVP
- Multi-language support
- Multi-currency pricing
- Multiple payment methods (PayPal, bank transfer, etc.)
- Guest checkout
- Product variants management
- Promotions and discount engine
- Product reviews and ratings
- Seller reputation system
- Advanced analytics dashboard
- Mobile applications
- Social media integration
- Wishlist functionality
- Product recommendations
- Advanced search filters
- Seller messaging
- Bulk product upload
- API for third-party integrations

---

## 9. Success Metrics for MVP

### 9.1 Technical Metrics
- ✓ System uptime: >99%
- ✓ API response time: <300ms for 95% of requests
- ✓ Payment success rate: >98%
- ✓ Order processing success rate: >99%

### 9.2 Business Metrics
- ✓ Seller onboarding time: <5 minutes
- ✓ Product listing time: <2 minutes
- ✓ Checkout completion rate: >70%
- ✓ Time to first payout: <21 days from first sale
- ✓ Payment processing errors: <2%

### 9.3 User Metrics
- ✓ First 50 sellers onboarded
- ✓ At least 5,000 SKUs in catalog
- ✓ At least 300 monthly active buyers within 3 months
- ✓ Average order value: >$50
- ✓ Repeat purchase rate: >20% within first 3 months

---

## 10. Critical Business Workflows Priority

For MVP implementation, prioritize in this order:

1. **Foundation** (Week 1-2)
   - User registration and authentication
   - Basic role management (Buyer, Seller, Admin)
   - Email verification

2. **Seller & Products** (Week 3-4)
   - Store profile creation
   - Product management (CRUD)
   - Category management
   - Product image upload

3. **Buyer Experience** (Week 5-6)
   - Product catalog browsing
   - Product search and filtering
   - Shopping cart management
   - User account management

4. **Checkout & Payment** (Week 7-8)
   - Checkout flow
   - Payment gateway integration
   - Order creation
   - Payment transaction tracking

5. **Order Management** (Week 9-10)
   - Order history (buyer side)
   - SubOrder management (seller side)
   - Order status updates
   - Shipment tracking

6. **Payout Processing** (Week 11-12)
   - Payout calculation
   - Payout scheduling
   - Seller payout dashboard
   - Transaction reconciliation

---

## 11. Integration Points

### 11.1 Payment Gateway Integration
- **Provider**: TBD (Stripe recommended)
- **Integration Type**: Server-to-server API
- **Required Capabilities**:
  - Payment processing
  - Payment status webhooks
  - Refund processing (future)
  - Transaction reporting

### 11.2 Shipping Provider Integration (Future)
- **Provider**: TBD
- **Integration Type**: API for tracking lookup
- **Required Capabilities**:
  - Tracking number validation
  - Delivery status updates
  - Estimated delivery dates

### 11.3 Email Service Integration
- **Provider**: TBD (SendGrid recommended)
- **Integration Type**: SMTP or API
- **Required Capabilities**:
  - Transactional emails
  - Email templates
  - Delivery tracking
  - Bounce handling

---

## 12. Reference

This document should be used as the primary source of truth for:
- Understanding business requirements
- Making technical decisions
- Resolving ambiguities
- Onboarding new team members
- Reviewing feature implementations

For technical implementation details, refer to:
- `architecture.md` - Technical architecture
- `prd.md` - Product requirements
- Module-specific documentation in `src/Modules/*/README.md` (to be created)

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-20  
**Status**: Living document - will be updated as business rules are refined during MVP development
