# Mercato – Domain Model Reference

This document provides visual and structured reference for the Mercato domain model. It complements the business-domain.md with entity relationship diagrams and data flow illustrations.

---

## 1. Core Entity Relationships

```
┌─────────────────────────────────────────────────────────────────┐
│                     MERCATO DOMAIN MODEL                         │
└─────────────────────────────────────────────────────────────────┘

┌──────────┐         ┌──────────┐         ┌──────────┐
│   User   │────────>│  Store   │────────>│ Product  │
│          │  owns   │          │  has    │          │
│ - UserID │         │ - StoreID│         │- Product │
│ - Email  │         │ - Name   │         │  ID      │
│ - Role   │         │ - Owner  │         │- SKU     │
└──────────┘         └──────────┘         │- Price   │
     │                                     │- Stock   │
     │                                     └──────────┘
     │ creates                                   │
     │                                           │ contains
     ▼                                           ▼
┌──────────┐         ┌──────────┐         ┌──────────┐
│   Cart   │────────>│CartItem  │────────>│ Product  │
│          │  has    │          │references│          │
│ - CartID │         │ - Qty    │         │          │
│ - UserID │         │ - Price  │         │          │
└──────────┘         └──────────┘         └──────────┘
     │
     │ checkout
     ▼
┌──────────┐         ┌──────────┐         ┌──────────┐
│  Order   │────────>│ SubOrder │────────>│ SubOrder │
│          │  has    │          │  has    │   Item   │
│ - OrderID│         │- SubOrder│         │          │
│ - Total  │         │  ID      │         │- Product │
│ - Status │         │- StoreID │         │- Qty     │
└──────────┘         │- Status  │         │- Price   │
     │               └──────────┘         └──────────┘
     │                     │
     │                     │ belongs to
     ▼                     ▼
┌──────────┐         ┌──────────┐
│ Payment  │         │  Store   │
│Transaction│        │          │
│          │         │          │
│ - Amount │         │          │
│ - Status │         └──────────┘
└──────────┘               │
                           │ receives
                           ▼
                     ┌──────────┐
                     │  Payout  │
                     │          │
                     │ - Amount │
                     │ - Status │
                     └──────────┘
```

---

## 2. User Role Hierarchy

```
┌────────────────────────────────────────────┐
│              USER ROLES                     │
└────────────────────────────────────────────┘

                    User
                      │
        ┌─────────────┼─────────────┐
        │             │             │
      Buyer        Seller         Admin
        │             │             │
        │             │             │
    Browse &       Manage        Manage
    Purchase       Store      Platform
        │             │             │
    - View         - Add         - User
      Catalog        Products      Management
    - Cart         - Process     - Category
    - Checkout       Orders        Config
    - Track        - View        - Commission
      Orders         Sales         Rates
                   - Payouts     - Reports
```

---

## 3. Order Status Flow

```
┌────────────────────────────────────────────────────────────┐
│                ORDER LIFECYCLE                              │
└────────────────────────────────────────────────────────────┘

BUYER VIEW (Order Level):
─────────────────────────
    ┌─────────┐      ┌─────────┐      ┌─────────┐
    │ Pending │─────>│  Paid   │─────>│Complete │
    └─────────┘      └─────────┘      └─────────┘
         │                                   ▲
         │                                   │
         ▼                                   │
    ┌─────────┐                             │
    │ Failed  │                             │
    └─────────┘                             │
                                            │
SELLER VIEW (SubOrder Level):               │
──────────────────────────────              │
                                            │
         ┌─────────────┐                    │
         │ Processing  │                    │
         └─────────────┘                    │
                │                           │
                │ seller ships              │
                ▼                           │
         ┌─────────────┐                    │
         │  Shipped    │                    │
         └─────────────┘                    │
                │                           │
                │ delivery confirmed        │
                ▼                           │
         ┌─────────────┐                    │
         │ Delivered   │────────────────────┘
         └─────────────┘
                │
                │ payout triggered
                ▼
         ┌─────────────┐
         │   Paid Out  │
         └─────────────┘
```

---

## 4. Payment Flow Diagram

```
┌────────────────────────────────────────────────────────────────┐
│              PAYMENT & PAYOUT FLOW                              │
└────────────────────────────────────────────────────────────────┘

    Buyer                Platform            Payment         Seller
      │                     │                Gateway            │
      │                     │                   │               │
      │  1. Checkout        │                   │               │
      │────────────────────>│                   │               │
      │                     │                   │               │
      │  2. Redirect to Pay │                   │               │
      │<────────────────────│                   │               │
      │                     │                   │               │
      │  3. Process Payment │                   │               │
      │─────────────────────┼──────────────────>│               │
      │                     │                   │               │
      │  4. Payment Success │                   │               │
      │<────────────────────┼───────────────────│               │
      │                     │                   │               │
      │  5. Order Created   │  6. Notify Seller │               │
      │<────────────────────│──────────────────────────────────>│
      │                     │                   │               │
      │                     │  ESCROW PERIOD    │               │
      │                     │  (awaiting delivery)              │
      │                     │                   │               │
      │                     │  7. Seller Ships  │               │
      │<────────────────────┼───────────────────────────────────│
      │                     │                   │               │
      │  8. Confirm Delivery│                   │               │
      │────────────────────>│                   │               │
      │                     │                   │               │
      │                     │  9. Calculate Payout             │
      │                     │  (Total - Commission - Fees)      │
      │                     │                   │               │
      │                     │  10. Transfer Funds              │
      │                     │──────────────────────────────────>│
      │                     │                   │               │
```

**Key Amounts**:
```
Order Total:          $110.00  (buyer pays)
├─ Product Total:     $100.00
└─ Shipping:           $10.00

Platform receives:    $110.00

Seller receives:       $91.51
├─ Gross:             $110.00
├─ Commission (15%):  -$15.00  (on product only)
└─ Processing Fee:     -$3.49  (2.9% + $0.30)

Platform keeps:        $18.49
├─ Commission:         $15.00
└─ Processing Fee:      $3.49
```

---

## 5. Multi-Seller Cart Structure

```
┌────────────────────────────────────────────────────────┐
│              BUYER'S CART (Multi-Seller)                │
└────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ Cart for User: john@example.com                     │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─ Seller: TechStore ─────────────────────────┐   │
│  │                                              │   │
│  │  • Product: Wireless Mouse                  │   │
│  │    Qty: 2 × $25.00 = $50.00                 │   │
│  │                                              │   │
│  │  • Product: USB Cable                       │   │
│  │    Qty: 3 × $5.00 = $15.00                  │   │
│  │                                              │   │
│  │  Subtotal: $65.00                           │   │
│  │  Shipping: $8.00 (Standard)                 │   │
│  │  Total: $73.00                              │   │
│  └──────────────────────────────────────────────┘   │
│                                                     │
│  ┌─ Seller: HomeGoods ──────────────────────────┐   │
│  │                                              │   │
│  │  • Product: Coffee Mug Set                  │   │
│  │    Qty: 1 × $30.00 = $30.00                 │   │
│  │                                              │   │
│  │  Subtotal: $30.00                           │   │
│  │  Shipping: $5.00 (Standard)                 │   │
│  │  Total: $35.00                              │   │
│  └──────────────────────────────────────────────┘   │
│                                                     │
├─────────────────────────────────────────────────────┤
│  CART TOTAL: $108.00                                │
└─────────────────────────────────────────────────────┘

After checkout, this creates:
  • 1 Order (OrderID: MKT-2024-000123, Total: $108.00)
    ├─ SubOrder 1 (for TechStore, $73.00)
    └─ SubOrder 2 (for HomeGoods, $35.00)
```

---

## 6. Data Access Boundaries

```
┌────────────────────────────────────────────────────────┐
│           MODULE DATA OWNERSHIP                         │
└────────────────────────────────────────────────────────┘

┌──────────────────┐
│     Users        │ Owns: User, Role, Authentication
│     Module       │ Exposes: IUserService
└──────────────────┘      - GetUser()
                          - ValidateCredentials()
                          
┌──────────────────┐
│  SellerPanel     │ Owns: Store, StoreSettings
│     Module       │ Exposes: IStoreService
└──────────────────┘      - CreateStore()
                          - GetStoreInfo()

┌──────────────────┐
│ ProductCatalog   │ Owns: Product, Category, ProductImage
│     Module       │ Exposes: IProductService, ICategoryService
└──────────────────┘      - AddProduct()
                          - SearchProducts()

┌──────────────────┐
│      Cart        │ Owns: Cart, CartItem
│     Module       │ Exposes: ICartService
└──────────────────┘      - AddToCart()
                          - GetCart()

┌──────────────────┐
│    Payments      │ Owns: PaymentTransaction, Payout
│     Module       │ Exposes: IPaymentService, IPayoutService
└──────────────────┘      - ProcessPayment()
                          - CalculatePayout()

┌──────────────────┐
│    History       │ Owns: Order, SubOrder, OrderItem
│     Module       │ Exposes: IOrderService
└──────────────────┘      - CreateOrder()
                          - GetOrderHistory()

┌──────────────────┐
│    Shipping      │ Owns: ShippingMethod, ShippingRate
│     Module       │ Exposes: IShippingService
└──────────────────┘      - CalculateShipping()
                          - GetTrackingInfo()

┌──────────────────┐
│  Notification    │ Owns: NotificationTemplate, Message
│     Module       │ Exposes: INotificationService
└──────────────────┘      - SendEmail()
                          - NotifyUser()

┌──────────────────┐
│    Reports       │ Owns: ReportDefinition, Analytics
│     Module       │ Exposes: IReportService
└──────────────────┘      - GetSalesReport()
                          - GetSellerStats()

RULE: Modules NEVER directly access other modules' tables.
      All cross-module communication via interfaces.
```

---

## 7. Key Business Calculations

### 7.1 Payout Calculation

```
┌────────────────────────────────────────────────────┐
│           SELLER PAYOUT FORMULA                     │
└────────────────────────────────────────────────────┘

INPUT:
  SubOrder Product Total:    P = $100.00
  SubOrder Shipping:         S = $10.00
  Gross Amount:              G = P + S = $110.00

FEES:
  Commission Rate:           C = 15% (0.15)
  Payment Gateway Rate:      R = 2.9% (0.029)
  Payment Gateway Fixed:     F = $0.30

CALCULATION:
  Platform Commission:       PC = P × C = $100.00 × 0.15 = $15.00
  Payment Processing Fee:    PF = (G × R) + F 
                                = ($110.00 × 0.029) + $0.30
                                = $3.19 + $0.30 
                                = $3.49
  
  Net Payout:                N = G - PC - PF
                                = $110.00 - $15.00 - $3.49
                                = $91.51

OUTPUT:
  Seller receives:           $91.51
  Platform keeps:            $18.49 ($15.00 commission + $3.49 gateway fee)
```

### 7.2 Stock Deduction Logic

```
┌────────────────────────────────────────────────────┐
│         STOCK MANAGEMENT RULES                      │
└────────────────────────────────────────────────────┘

SCENARIO 1: Adding to Cart
  Current Stock:    50 units
  Cart Quantity:     5 units
  ────────────────────────────
  Stock Reserved:    0 units (stock not reserved on add to cart)
  Available Stock:  50 units
  Status:           ✓ Allowed

SCENARIO 2: Checkout Validation
  Current Stock:    50 units
  Cart Quantity:     5 units
  ────────────────────────────
  Validation:       ✓ Pass (5 ≤ 50)
  Action:           Proceed to payment

SCENARIO 3: Payment Success (Order Creation)
  Current Stock:    50 units
  Order Quantity:    5 units
  ────────────────────────────
  Stock Deducted:    5 units
  New Stock:        45 units
  Status:           ✓ Order Created

SCENARIO 4: Concurrent Orders (Race Condition)
  Initial Stock:    10 units
  Order A requests:  8 units (at t=0)
  Order B requests:  5 units (at t=0.1s)
  ────────────────────────────
  Solution:         Database row-level locking
  Order A:          ✓ Success (stock: 10 → 2)
  Order B:          ✗ Failed (insufficient stock: 2 < 5)
```

---

## 8. Module Dependencies

```
┌────────────────────────────────────────────────────┐
│        MODULE DEPENDENCY GRAPH                      │
└────────────────────────────────────────────────────┘

             ┌─────────────────┐
             │   API Layer     │
             └─────────────────┘
                      │
         ┌────────────┼────────────┐
         │            │            │
         ▼            ▼            ▼
    ┌────────┐  ┌────────┐  ┌────────┐
    │ Users  │  │Product │  │ Seller │
    │        │  │Catalog │  │ Panel  │
    └────────┘  └────────┘  └────────┘
         │            │            │
         │            │            │
         ▼            ▼            ▼
    ┌─────────────────────────────────┐
    │           Cart                  │
    └─────────────────────────────────┘
                      │
                      ▼
    ┌─────────────────────────────────┐
    │         Shipping                │
    └─────────────────────────────────┘
                      │
                      ▼
    ┌─────────────────────────────────┐
    │         Payments                │
    └─────────────────────────────────┘
                      │
                      ▼
    ┌─────────────────────────────────┐
    │         History (Orders)        │
    └─────────────────────────────────┘
                      │
         ┌────────────┼────────────┐
         ▼            ▼            ▼
    ┌────────┐  ┌────────┐  ┌────────┐
    │Notific.│  │Reports │  │Shipping│
    └────────┘  └────────┘  └────────┘

LEGEND:
  ──> Direct dependency (uses interfaces from)
  
KEY PRINCIPLE: 
  Dependencies flow downward and sideways, never upward.
  Lower modules don't know about higher modules.
```

---

## 9. State Machines

### 9.1 SubOrder State Machine

```
┌────────────────────────────────────────────────────┐
│        SUBORDER STATE TRANSITIONS                   │
└────────────────────────────────────────────────────┘

                 ┌─────────────┐
           ┌────>│ Processing  │<────┐
           │     └─────────────┘     │
           │            │            │
           │            │ Ship()     │
           │            ▼            │
           │     ┌─────────────┐    │
    Create()     │   Shipped   │    │ CancelShipment()
           │     └─────────────┘    │ (before delivery)
           │            │            │
           │            │ Confirm()  │
           │            │ or         │
           │            │ AutoConfirm(14d)
           │            ▼            │
           │     ┌─────────────┐    │
           └─────│ Delivered   │────┘
                 └─────────────┘
                        │
                        │ MarkPaid()
                        ▼
                 ┌─────────────┐
                 │  Paid Out   │
                 └─────────────┘

ALLOWED TRANSITIONS:
  Processing → Shipped       (Seller ships order)
  Shipped → Delivered        (Buyer confirms or auto-confirm)
  Delivered → Paid Out       (Platform processes payout)
  
FORBIDDEN TRANSITIONS:
  Shipped → Processing       (Cannot unship)
  Delivered → Shipped        (Cannot undeliver)
  * → Processing             (Cannot restart)
```

### 9.2 Payment Transaction State Machine

```
┌────────────────────────────────────────────────────┐
│    PAYMENT TRANSACTION STATE TRANSITIONS            │
└────────────────────────────────────────────────────┘

                 ┌─────────────┐
           ┌────>│   Pending   │
           │     └─────────────┘
           │            │
    Create()            │ Gateway
           │            │ Response
           │     ┌──────┴──────┐
           │     ▼             ▼
           │ ┌─────────┐  ┌─────────┐
           │ │Completed│  │ Failed  │
           │ └─────────┘  └─────────┘
           │      │             │
           │      │             │ Retry
           │      │ Refund()    │ Payment
           │      ▼             │
           │ ┌─────────┐        │
           └─│Refunded │        │
             └─────────┘        │
                   ▲            │
                   │            │
                   └────────────┘

ALLOWED TRANSITIONS:
  Pending → Completed      (Payment successful)
  Pending → Failed         (Payment declined)
  Completed → Refunded     (Refund processed)
  Failed → Pending         (Retry payment)
  
FORBIDDEN TRANSITIONS:
  Completed → Pending      (Cannot undo successful payment)
  Failed → Completed       (Must go through Pending)
  Refunded → *             (Terminal state)
```

---

## 10. API Endpoint Structure (Planned)

```
┌────────────────────────────────────────────────────┐
│              REST API ENDPOINTS                     │
└────────────────────────────────────────────────────┘

AUTHENTICATION & USERS
  POST   /api/auth/register
  POST   /api/auth/login
  POST   /api/auth/logout
  POST   /api/auth/verify-email
  GET    /api/users/profile
  PUT    /api/users/profile

SELLERS & STORES
  POST   /api/sellers/stores
  GET    /api/sellers/stores/{storeId}
  PUT    /api/sellers/stores/{storeId}
  GET    /api/sellers/stores/{storeId}/stats

PRODUCTS
  POST   /api/products
  GET    /api/products/{productId}
  PUT    /api/products/{productId}
  DELETE /api/products/{productId}
  GET    /api/products                    (search/filter)
  GET    /api/sellers/stores/{storeId}/products

CATEGORIES
  GET    /api/categories
  GET    /api/categories/{categoryId}
  POST   /api/admin/categories            (admin only)

CART
  GET    /api/cart
  POST   /api/cart/items
  PUT    /api/cart/items/{itemId}
  DELETE /api/cart/items/{itemId}
  DELETE /api/cart                        (clear cart)

CHECKOUT & ORDERS
  POST   /api/checkout/validate
  POST   /api/checkout/create-order
  GET    /api/orders
  GET    /api/orders/{orderId}
  GET    /api/sellers/orders              (seller's orders)
  PUT    /api/sellers/orders/{subOrderId}/ship

PAYMENTS
  POST   /api/payments/process
  GET    /api/payments/transactions/{transactionId}
  POST   /api/payments/webhook            (payment gateway callback)

PAYOUTS (SELLER)
  GET    /api/sellers/payouts
  GET    /api/sellers/payouts/{payoutId}

SHIPPING
  GET    /api/shipping/methods
  POST   /api/shipping/calculate
  GET    /api/shipping/track/{trackingNumber}

REPORTS (ADMIN & SELLER)
  GET    /api/reports/sales               (seller sales)
  GET    /api/admin/reports/platform      (platform-wide)
  GET    /api/admin/reports/commissions
```

---

## 11. Database Schema Overview

```
┌────────────────────────────────────────────────────┐
│           DATABASE TABLES (Simplified)              │
└────────────────────────────────────────────────────┘

Users Module:
  └─ Users
       ├─ UserID (PK)
       ├─ Email (unique)
       ├─ PasswordHash
       ├─ Role
       └─ CreatedAt

SellerPanel Module:
  └─ Stores
       ├─ StoreID (PK)
       ├─ OwnerUserID (FK → Users)
       ├─ StoreName (unique)
       ├─ CommissionRate
       └─ IsActive

ProductCatalog Module:
  ├─ Categories
  │    ├─ CategoryID (PK)
  │    ├─ Name
  │    └─ ParentCategoryID (FK → Categories)
  │
  └─ Products
       ├─ ProductID (PK)
       ├─ StoreID (FK → Stores)
       ├─ CategoryID (FK → Categories)
       ├─ SKU (unique within store)
       ├─ Title
       ├─ Price
       ├─ StockQuantity
       └─ Status

Cart Module:
  ├─ Carts
  │    ├─ CartID (PK)
  │    ├─ UserID (FK → Users)
  │    └─ ExpiresAt
  │
  └─ CartItems
       ├─ CartItemID (PK)
       ├─ CartID (FK → Carts)
       ├─ ProductID (FK → Products)
       ├─ Quantity
       └─ PriceAtAdd

History Module:
  ├─ Orders
  │    ├─ OrderID (PK)
  │    ├─ OrderNumber (unique)
  │    ├─ UserID (FK → Users)
  │    ├─ TotalAmount
  │    ├─ PaymentStatus
  │    └─ CreatedAt
  │
  ├─ SubOrders
  │    ├─ SubOrderID (PK)
  │    ├─ SubOrderNumber (unique)
  │    ├─ OrderID (FK → Orders)
  │    ├─ StoreID (FK → Stores)
  │    ├─ Status
  │    ├─ ShippingMethod
  │    └─ TrackingNumber
  │
  └─ SubOrderItems
       ├─ SubOrderItemID (PK)
       ├─ SubOrderID (FK → SubOrders)
       ├─ ProductID (FK → Products)
       ├─ Quantity
       └─ PricePerUnit

Payments Module:
  ├─ PaymentTransactions
  │    ├─ TransactionID (PK)
  │    ├─ OrderID (FK → Orders)
  │    ├─ Amount
  │    ├─ Status
  │    └─ PaymentGatewayID
  │
  └─ Payouts
       ├─ PayoutID (PK)
       ├─ StoreID (FK → Stores)
       ├─ GrossAmount
       ├─ CommissionAmount
       ├─ NetAmount
       ├─ Status
       └─ ScheduledAt

Shipping Module:
  └─ ShippingMethods
       ├─ ShippingMethodID (PK)
       ├─ Name
       ├─ BaseRate
       └─ IsActive

Notification Module:
  └─ NotificationLogs
       ├─ NotificationID (PK)
       ├─ UserID (FK → Users)
       ├─ Type
       ├─ Status
       └─ SentAt
```

---

## 12. Event Flow Example: Complete Purchase

```
┌────────────────────────────────────────────────────────────────┐
│          EVENT FLOW: BUYER PURCHASES FROM 2 SELLERS             │
└────────────────────────────────────────────────────────────────┘

1. CART PREPARATION
   ┌──────────────────────────────────────────┐
   │ Buyer adds items to cart:                │
   │  - Product A from Store 1 ($50)          │
   │  - Product B from Store 2 ($30)          │
   └──────────────────────────────────────────┘
   
2. CHECKOUT INITIATED
   ┌──────────────────────────────────────────┐
   │ Cart.ValidateItems()                     │
   │  ✓ All items in stock                    │
   │  ✓ Prices current                        │
   │                                          │
   │ Shipping.CalculateRates()                │
   │  Store 1 shipping: $10                   │
   │  Store 2 shipping: $5                    │
   │                                          │
   │ Total: $95                               │
   └──────────────────────────────────────────┘

3. PAYMENT PROCESSING
   ┌──────────────────────────────────────────┐
   │ Payments.ProcessPayment($95)             │
   │  → Gateway charges card                  │
   │  → Success!                              │
   │                                          │
   │ PaymentTransaction created:              │
   │  - Status: Completed                     │
   │  - Amount: $95                           │
   └──────────────────────────────────────────┘

4. ORDER CREATION
   ┌──────────────────────────────────────────┐
   │ History.CreateOrder()                    │
   │                                          │
   │ Order created:                           │
   │  - OrderNumber: MKT-2024-000100          │
   │  - Total: $95                            │
   │  - Status: Paid                          │
   │                                          │
   │ SubOrder 1 created (Store 1):            │
   │  - SubOrderNumber: SUB-2024-000200       │
   │  - Total: $60 ($50 + $10 shipping)       │
   │  - Status: Processing                    │
   │                                          │
   │ SubOrder 2 created (Store 2):            │
   │  - SubOrderNumber: SUB-2024-000201       │
   │  - Total: $35 ($30 + $5 shipping)        │
   │  - Status: Processing                    │
   └──────────────────────────────────────────┘

5. STOCK DEDUCTION
   ┌──────────────────────────────────────────┐
   │ ProductCatalog.DeductStock()             │
   │  Product A: 100 → 99 units               │
   │  Product B: 50 → 49 units                │
   └──────────────────────────────────────────┘

6. NOTIFICATIONS SENT
   ┌──────────────────────────────────────────┐
   │ Notification.SendEmail(Buyer)            │
   │  "Order MKT-2024-000100 confirmed"       │
   │                                          │
   │ Notification.SendEmail(Store 1 Owner)    │
   │  "New order SUB-2024-000200"             │
   │                                          │
   │ Notification.SendEmail(Store 2 Owner)    │
   │  "New order SUB-2024-000201"             │
   └──────────────────────────────────────────┘

7. SELLER FULFILLMENT (days later)
   ┌──────────────────────────────────────────┐
   │ Store 1: Ships SubOrder 200              │
   │  - Status: Processing → Shipped          │
   │  - Tracking: 1Z999AA10123456784          │
   │                                          │
   │ Store 2: Ships SubOrder 201              │
   │  - Status: Processing → Shipped          │
   │  - Tracking: 1Z999AA10123456785          │
   └──────────────────────────────────────────┘

8. DELIVERY CONFIRMATION (14 days later)
   ┌──────────────────────────────────────────┐
   │ Auto-confirmation triggers:              │
   │  SubOrder 200: Shipped → Delivered       │
   │  SubOrder 201: Shipped → Delivered       │
   └──────────────────────────────────────────┘

9. PAYOUT CALCULATION (next Monday)
   ┌──────────────────────────────────────────┐
   │ Payments.CalculatePayout(Store 1)        │
   │  Gross: $60                              │
   │  Commission: $50 × 15% = $7.50           │
   │  Processing: $60 × 2.9% + $0.30 = $2.04  │
   │  Net: $60 - $7.50 - $2.04 = $50.46       │
   │                                          │
   │ Payments.CalculatePayout(Store 2)        │
   │  Gross: $35                              │
   │  Commission: $30 × 15% = $4.50           │
   │  Processing: $35 × 2.9% + $0.30 = $1.32  │
   │  Net: $35 - $4.50 - $1.32 = $29.18       │
   │                                          │
   │ Payout 1: $50.46 → Store 1 bank          │
   │ Payout 2: $29.18 → Store 2 bank          │
   └──────────────────────────────────────────┘

FINAL STATE:
  ✓ Buyer received both orders
  ✓ Store 1 received $50.46
  ✓ Store 2 received $29.18
  ✓ Platform earned $11.36 ($7.50 + $4.50 - $0.64 gateway profit)
```

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-20  
**Maintained by**: Architecture Team
