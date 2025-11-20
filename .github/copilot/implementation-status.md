# Mercato MVP - Implementation Tracking

This document tracks the implementation status of the Mercato MVP features against the defined business scope.

---

## 📊 Overall Progress

**Last Updated:** 2025-11-20  
**Status:** Users Module Complete - Authentication & Authorization Implemented  
**Overall Completion:** 15% (Foundation + Users Module)

---

## 🎯 MVP Milestones

| Milestone | Target Week | Status | Completion |
|-----------|-------------|--------|------------|
| 1. Foundation & Documentation | Week 0 | ✅ Complete | 100% |
| 2. User & Seller Onboarding | Week 1-2 | ✅ Authentication Complete, Seller Onboarding TODO | 50% |
| 3. Product Management | Week 3-4 | 🔄 Not Started | 0% |
| 4. Buyer Experience | Week 5-6 | 🔄 Not Started | 0% |
| 5. Checkout & Payment | Week 7-8 | 🔄 Not Started | 0% |
| 6. Order Management | Week 9-10 | 🔄 Not Started | 0% |
| 7. Payout Processing | Week 11-12 | 🔄 Not Started | 0% |

---

## 📋 Feature Implementation Status

### 1. Foundation & Infrastructure ✅

#### Documentation
- [x] Business domain documentation created
- [x] Domain model reference created
- [x] Quick start guide created
- [x] PRD reviewed and validated
- [x] Architecture documentation reviewed and updated (auth section)
- [x] API documentation created (AUTH endpoints)
- [x] Database setup guide created
- [ ] Swagger/OpenAPI documentation (TODO)

#### Project Setup
- [x] Solution structure validated
- [x] Module projects created
- [x] Build system verified
- [x] Database setup (EF Core migrations created)
- [x] JWT authentication configured
- [x] OAuth providers configured (placeholders)
- [ ] Logging infrastructure (basic logging present, structured logging TODO)
- [ ] Exception handling middleware (TODO)
- [ ] Application Insights integration (TODO)

---

### 2. Users Module ✅ Complete

#### Authentication & Authorization
- [x] User entity and DbContext
- [x] User registration endpoint
- [x] Email verification flow (auto-verified in MVP)
- [x] Login/logout endpoints
- [x] Password hashing (PBKDF2 via ASP.NET Core Identity)
- [x] JWT token generation
- [x] Role-based authorization (Buyer, Seller, Administrator)
- [x] OAuth 2.0 integration (Google, Facebook)
- [x] SellerStaff entity for future multi-user stores
- [ ] Password reset flow (TODO)
- [ ] Email verification with actual emails (TODO)

**Frontend:**
- [x] Login page with email/password
- [x] Registration page with role selection
- [x] Profile page for viewing/editing user info
- [x] AuthDisplay component with role-based menus
- [x] Authentication state management (LocalStorage + JWT)
- [x] Social login buttons (UI only, full OAuth flow TODO)

**Tests:**
- [ ] Unit tests for user validation
- [ ] Integration tests for auth endpoints
- [ ] Email verification tests

**Business Rules Implemented:**
- [x] Email uniqueness validation
- [x] Password complexity requirements (8 chars, uppercase, lowercase, digit)
- [ ] Age requirement validation (18+) - TODO
- [x] Email auto-verification (production should use actual emails)
- [x] Role assignment (Buyer, Seller, Administrator)
- [x] JWT token with claims
- [x] Seller staff extensibility model

---

### 3. Seller Panel Module 🔄 Not Started

#### Store Management
- [ ] Store entity and DbContext
- [ ] Create store endpoint
- [ ] Store profile management
- [ ] Store name uniqueness validation
- [ ] Bank account details management
- [ ] Commission rate configuration (admin)
- [ ] Store activation/deactivation

**Tests:**
- [ ] Unit tests for store validation
- [ ] Integration tests for store CRUD
- [ ] Store name uniqueness tests

**Business Rules Implemented:**
- [ ] One store per user (MVP)
- [ ] Store name uniqueness
- [ ] Required fields validation
- [ ] Bank account validation

---

### 4. Product Catalog Module 🔄 Not Started

#### Category Management
- [ ] Category entity and DbContext
- [ ] Category CRUD endpoints (admin)
- [ ] Category hierarchy support (max 3 levels)
- [ ] Category activation/deactivation

#### Product Management
- [ ] Product entity and DbContext
- [ ] Product CRUD endpoints
- [ ] SKU uniqueness per store
- [ ] Product image upload
- [ ] Product image storage (blob/filesystem)
- [ ] Stock quantity management
- [ ] Product status (Draft/Published/Archived)
- [ ] Product search and filtering
- [ ] Product listing by category

**Tests:**
- [ ] Unit tests for product validation
- [ ] Integration tests for product CRUD
- [ ] SKU uniqueness tests
- [ ] Stock management tests
- [ ] Search functionality tests

**Business Rules Implemented:**
- [ ] SKU unique within store
- [ ] Price > 0 validation
- [ ] Stock >= 0 validation
- [ ] At least 1 image for published products
- [ ] Max 10 images per product
- [ ] Image size and format validation
- [ ] Title/description length limits

---

### 5. Cart Module 🔄 Not Started

#### Shopping Cart
- [ ] Cart and CartItem entities
- [ ] Cart DbContext
- [ ] Get cart endpoint
- [ ] Add to cart endpoint
- [ ] Update cart item endpoint
- [ ] Remove cart item endpoint
- [ ] Clear cart endpoint
- [ ] Cart expiration handling (30 days)
- [ ] Cart validation (stock, prices)
- [ ] Cart grouping by seller

**Tests:**
- [ ] Unit tests for cart operations
- [ ] Integration tests for cart endpoints
- [ ] Cart validation tests
- [ ] Concurrent cart operation tests

**Business Rules Implemented:**
- [ ] One cart per user
- [ ] Max 50 unique items
- [ ] Cart expiration after 30 days
- [ ] Quantity <= available stock
- [ ] Price snapshot at add time

---

### 6. Shipping Module 🔄 Not Started

#### Shipping Methods
- [ ] ShippingMethod entity and DbContext
- [ ] Shipping method CRUD (admin)
- [ ] Shipping rate calculation
- [ ] Shipping method selection
- [ ] Tracking number validation

**Tests:**
- [ ] Unit tests for shipping calculation
- [ ] Integration tests for shipping endpoints

**Business Rules Implemented:**
- [ ] Shipping cost calculation
- [ ] Tracking number format validation

---

### 7. Payments Module 🔄 Not Started

#### Payment Processing
- [ ] PaymentTransaction entity and DbContext
- [ ] Payment gateway integration (Stripe/PayU)
- [ ] Payment processing endpoint
- [ ] Payment status webhook
- [ ] Payment success handling
- [ ] Payment failure handling
- [ ] Transaction logging

#### Payout Processing
- [ ] Payout entity and DbContext
- [ ] Payout calculation logic
- [ ] Commission calculation
- [ ] Processing fee calculation
- [ ] Payout scheduling (weekly)
- [ ] Payout execution
- [ ] Payout status tracking
- [ ] Minimum payout threshold ($10)

**Tests:**
- [ ] Unit tests for payout calculations
- [ ] Integration tests for payment flow
- [ ] Commission calculation tests
- [ ] Processing fee calculation tests
- [ ] Payment gateway webhook tests

**Business Rules Implemented:**
- [ ] Commission rate: 15% on products
- [ ] Processing fee: 2.9% + $0.30
- [ ] Payout after delivery confirmation
- [ ] Auto-confirmation after 14 days
- [ ] Minimum payout: $10
- [ ] Weekly payout schedule (Mondays)

---

### 8. History (Orders) Module 🔄 Not Started

#### Order Management
- [ ] Order and SubOrder entities
- [ ] OrderItem and SubOrderItem entities
- [ ] Order DbContext
- [ ] Create order from cart
- [ ] Order number generation
- [ ] SubOrder number generation
- [ ] Stock deduction on order creation
- [ ] Order status tracking
- [ ] SubOrder status tracking
- [ ] Order history endpoint (buyer)
- [ ] SubOrder management endpoint (seller)
- [ ] Update SubOrder status (ship)
- [ ] Add tracking number
- [ ] Delivery confirmation
- [ ] Auto-delivery confirmation (14 days)

**Tests:**
- [ ] Unit tests for order creation
- [ ] Integration tests for order flow
- [ ] Stock deduction tests
- [ ] Order status transition tests
- [ ] Concurrent order tests (stock locking)

**Business Rules Implemented:**
- [ ] Order created only after payment
- [ ] Stock deducted at order creation
- [ ] One order per checkout
- [ ] Multiple SubOrders per seller
- [ ] SubOrder status flow validation
- [ ] Seller must ship within 3 days
- [ ] Auto-delivery after 14 days

---

### 9. Notification Module 🔄 Not Started

#### Email Notifications
- [ ] NotificationLog entity and DbContext
- [ ] Email service integration (SendGrid)
- [ ] Email templates
- [ ] Order confirmation email (buyer)
- [ ] New order notification (seller)
- [ ] Shipment notification (buyer)
- [ ] Delivery confirmation email
- [ ] Payout notification (seller)
- [ ] Email verification email
- [ ] Password reset email

**Tests:**
- [ ] Unit tests for notification logic
- [ ] Integration tests for email sending
- [ ] Email template rendering tests

**Business Rules Implemented:**
- [ ] Notification for key events
- [ ] Email delivery tracking

---

### 10. Reports Module 🔄 Not Started

#### Seller Reports
- [ ] Sales report endpoint
- [ ] Payout history endpoint
- [ ] Product performance metrics

#### Admin Reports
- [ ] Platform-wide sales report
- [ ] Commission report
- [ ] Seller performance metrics
- [ ] Transaction reconciliation report

**Tests:**
- [ ] Unit tests for report calculations
- [ ] Integration tests for report endpoints

**Business Rules Implemented:**
- [ ] Accurate financial reporting
- [ ] Transaction reconciliation

---

## 🔧 API Endpoints Status

### Authentication & Users
- [x] POST /api/auth/register
- [x] POST /api/auth/login
- [x] POST /api/auth/logout
- [x] POST /api/auth/external-login
- [ ] POST /api/auth/verify-email (TODO)
- [ ] POST /api/auth/forgot-password (TODO)
- [ ] POST /api/auth/reset-password (TODO)
- [x] GET /api/users/profile
- [x] PUT /api/users/profile

### Sellers & Stores
- [ ] POST /api/sellers/stores
- [ ] GET /api/sellers/stores/{storeId}
- [ ] PUT /api/sellers/stores/{storeId}
- [ ] GET /api/sellers/stores/{storeId}/stats

### Products
- [ ] POST /api/products
- [ ] GET /api/products/{productId}
- [ ] PUT /api/products/{productId}
- [ ] DELETE /api/products/{productId}
- [ ] GET /api/products (search/filter)
- [ ] GET /api/sellers/stores/{storeId}/products

### Categories
- [ ] GET /api/categories
- [ ] GET /api/categories/{categoryId}
- [ ] POST /api/admin/categories

### Cart
- [ ] GET /api/cart
- [ ] POST /api/cart/items
- [ ] PUT /api/cart/items/{itemId}
- [ ] DELETE /api/cart/items/{itemId}
- [ ] DELETE /api/cart

### Checkout & Orders
- [ ] POST /api/checkout/validate
- [ ] POST /api/checkout/create-order
- [ ] GET /api/orders
- [ ] GET /api/orders/{orderId}
- [ ] GET /api/sellers/orders
- [ ] PUT /api/sellers/orders/{subOrderId}/ship

### Payments
- [ ] POST /api/payments/process
- [ ] GET /api/payments/transactions/{transactionId}
- [ ] POST /api/payments/webhook

### Payouts
- [ ] GET /api/sellers/payouts
- [ ] GET /api/sellers/payouts/{payoutId}

### Shipping
- [ ] GET /api/shipping/methods
- [ ] POST /api/shipping/calculate
- [ ] GET /api/shipping/track/{trackingNumber}

### Reports
- [ ] GET /api/reports/sales
- [ ] GET /api/admin/reports/platform
- [ ] GET /api/admin/reports/commissions

---

## 🗄️ Database Migrations Status

- [x] Initial migration created for Users module
- [x] Users tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)
- [x] SellerStaff table (future-proof for multi-user stores)
- [ ] Stores tables
- [ ] Products and Categories tables
- [ ] Cart tables
- [ ] Orders and SubOrders tables
- [ ] Payment transactions tables
- [ ] Payout tables
- [ ] Shipping tables
- [ ] Notification tables
- [ ] Indexes created (basic indexes via Identity, custom indexes TODO)
- [ ] Foreign key constraints (within Users module complete, cross-module TODO)
- [ ] Seed data (roles seeded automatically, categories/shipping TODO)

---

## 🧪 Testing Status

### Unit Tests
- [ ] Users module: 0/10 tests
- [ ] SellerPanel module: 0/8 tests
- [ ] ProductCatalog module: 0/15 tests
- [ ] Cart module: 0/10 tests
- [ ] Payments module: 0/12 tests
- [ ] History module: 0/15 tests
- [ ] Shipping module: 0/5 tests
- [ ] Notification module: 0/5 tests
- [ ] Reports module: 0/5 tests

### Integration Tests
- [ ] API endpoint tests: 0/40 tests
- [ ] Database integration tests: 0/20 tests
- [ ] Payment gateway integration: 0/5 tests

### Manual Testing Scenarios
- [ ] End-to-end purchase flow
- [ ] Seller onboarding
- [ ] Product management
- [ ] Order fulfillment
- [ ] Payout processing

---

## 🚧 Known Technical Debt

*None yet - will be tracked as implementation progresses*

---

## 📝 Open Business Questions

From business-domain.md TODOs:

1. ❓ Seller approval process: Auto-approve or manual review?
2. ❓ Bank account minimum fields (country-specific)
3. ❓ Minimum product price requirement
4. ❓ Restricted product categories
5. ❓ Stock locking strategy in cart
6. ❓ Price change threshold at checkout (10% suggested)
7. ❓ Partial fulfillment support
8. ❓ Late shipment penalties
9. ❓ Return/refund fee handling

**Action Required:** Business stakeholder decisions needed before implementation.

---

## 🎉 Completed Items

### Week 0 - Foundation
- ✅ Repository structure analyzed
- ✅ Build system verified
- ✅ Business domain documentation created
- ✅ Domain model reference created
- ✅ Quick start guide created
- ✅ Implementation tracking document created
- ✅ README updated with documentation links

### Week 1 - Authentication & Authorization
- ✅ Users module implemented with ASP.NET Core Identity
- ✅ User, Role, and SellerStaff entities created
- ✅ Database context and migrations created
- ✅ JWT authentication configured
- ✅ OAuth 2.0 configured (Google, Facebook)
- ✅ Registration endpoint (email/password)
- ✅ Login endpoint (email/password)
- ✅ External login endpoint (OAuth)
- ✅ Logout endpoint
- ✅ User profile endpoints (GET, PUT)
- ✅ Role-based authorization with policies
- ✅ Frontend login page
- ✅ Frontend registration page
- ✅ Frontend profile page
- ✅ AuthDisplay component with role-based menus
- ✅ Authentication state management
- ✅ API documentation for auth endpoints
- ✅ Architecture documentation updated
- ✅ Database setup guide created

---

## 📅 Next Steps

### Immediate (Week 1-2)
1. ~~Set up database infrastructure~~ ✅ Complete
   - ~~Configure connection strings~~ ✅ Complete
   - ~~Set up EF Core migrations~~ ✅ Complete
   - ~~Create initial migration~~ ✅ Complete

2. ~~Implement Users module~~ ✅ Complete
   - ~~User entity and authentication~~ ✅ Complete
   - ~~Registration and login endpoints~~ ✅ Complete
   - ~~Email verification (auto-verified in MVP)~~ ✅ Complete

3. Implement Seller Panel module
   - Store entity and DbContext
   - Store registration/onboarding flow
   - Store profile management
   - Bank account details (basic validation)

4. Set up exception handling and logging
   - Exception handling middleware
   - Structured logging with Serilog
   - Request correlation IDs

### Short Term (Weeks 2-4)
1. Complete Seller Panel module
2. Implement Product Catalog module
3. Set up Swagger/OpenAPI documentation
4. Begin unit and integration testing
5. Add email service for verification (SendGrid/SMTP)

---

**Legend:**
- ✅ Complete
- 🔄 In Progress  
- ❌ Blocked
- ⏸️ Paused
- 🔄 Not Started

---

*This is a living document. Update regularly as features are implemented.*
