# Mercato MVP - Quick Start Guide for Developers

This guide helps you quickly understand the Mercato marketplace platform and where to find information for implementing features.

---

## 📚 Documentation Roadmap

### 1. Start Here: Business Understanding
**Read first:** [Business Domain Documentation](./business-domain.md)

This is your primary reference for:
- ✅ What the MVP does (core business flows)
- ✅ How payments work (escrow model)
- ✅ Business rules and validations
- ✅ Entity definitions and relationships
- ✅ Domain terminology (glossary)

**Time to read:** 30-45 minutes

### 2. Visual Reference: Domain Model
**Read second:** [Domain Model Reference](./domain-model.md)

Use this for:
- 📊 Entity relationship diagrams
- 🔄 State machines and workflows
- 💰 Payment calculation examples
- 🗺️ API endpoint structure
- 🏗️ Module architecture

**Time to read:** 20-30 minutes

### 3. Product Requirements
**Read third:** [PRD - Product Requirements](./prd.md)

Use this for:
- 🎯 Feature specifications
- ✔️ Acceptance criteria
- 📈 Success metrics
- 🚫 Out of scope items

**Time to read:** 15-20 minutes

### 4. Technical Architecture
**Read when implementing:** [Architecture Overview](./architecture.md)

Use this for:
- 🏛️ System architecture
- 🔌 Module boundaries
- 📡 API design patterns
- 💾 Data access strategy
- 🧪 Testing approach

**Time to read:** 20-30 minutes

---

## 🚀 MVP Implementation Priority

The MVP focuses on three core capabilities:

### Phase 1: Seller Onboarding & Product Listing (Weeks 1-4)
- User registration and authentication
- Store profile creation
- Product catalog management
- Category system

**Key Modules:** `Users`, `SellerPanel`, `ProductCatalog`

### Phase 2: Buyer Experience (Weeks 5-6)
- Product browsing and search
- Shopping cart
- User account management

**Key Modules:** `ProductCatalog`, `Cart`, `Users`

### Phase 3: Checkout & Payment (Weeks 7-8)
- Complete checkout flow
- Payment gateway integration
- Order creation
- Stock management

**Key Modules:** `Cart`, `Payments`, `History`

### Phase 4: Order Management (Weeks 9-10)
- Order tracking (buyer side)
- SubOrder fulfillment (seller side)
- Shipment tracking
- Delivery confirmation

**Key Modules:** `History`, `Shipping`, `Notification`

### Phase 5: Payout Processing (Weeks 11-12)
- Payout calculation
- Seller payout dashboard
- Transaction reconciliation
- Financial reporting

**Key Modules:** `Payments`, `Reports`

---

## 💡 Key Business Rules (Quick Reference)

### Products
- ✓ Price must be > $0
- ✓ SKU unique per store
- ✓ At least 1 image required
- ✓ Stock cannot be negative

### Cart
- ✓ Max 50 unique items
- ✓ Expires after 30 days
- ✓ One cart per user

### Orders
- ✓ Created only after successful payment
- ✓ Stock deducted at order creation
- ✓ One Order can have multiple SubOrders (one per seller)

### Payments & Payouts
- ✓ Platform commission: **15%** (on products only, not shipping)
- ✓ Processing fee: **2.9% + $0.30** per transaction
- ✓ Payout after delivery confirmation (or auto-confirm after 14 days)
- ✓ Minimum payout: **$10**
- ✓ Payout frequency: **Weekly (Mondays)**

**Example Payout Calculation:**
```
Order: $100 (products) + $10 (shipping) = $110
Commission: $100 × 15% = $15
Processing: $110 × 2.9% + $0.30 = $3.49
Seller receives: $110 - $15 - $3.49 = $91.51
```

### Shipping
- ✓ Seller must ship within 3 business days
- ✓ Tracking number recommended
- ✓ Auto-delivery confirmation after 14 days

---

## ❓ Open Questions (TODOs in Documentation)

The business domain documentation includes TODO comments for items that need business stakeholder clarification:

1. **Seller Approval**: Auto-approve or require admin review?
2. **Bank Account Details**: Minimum required fields (varies by country)
3. **Minimum Price**: Should there be a minimum to prevent $0.01 listings?
4. **Restricted Products**: Are there categories requiring approval?
5. **Stock Locking**: Lock stock in cart or notify at checkout?
6. **Price Changes**: What's acceptable threshold during checkout? (10% suggested)
7. **Partial Fulfillment**: Support if items become unavailable?
8. **Late Shipment**: Penalties or warning system?
9. **Returns/Refunds**: How are fees handled? (Future phase)

**Action:** Review TODOs and get business decisions before implementing those areas.

---

## 🏗️ Module Structure

```
src/Modules/
├── SD.Mercato.Users              → Authentication, users, roles
├── SD.Mercato.SellerPanel        → Store profiles, seller settings
├── SD.Mercato.ProductCatalog     → Products, categories, search
├── SD.Mercato.Cart               → Shopping cart, cart items
├── SD.Mercato.Payments           → Transactions, payouts, gateway
├── SD.Mercato.History            → Orders, SubOrders, history
├── SD.Mercato.Shipping           → Shipping methods, tracking
├── SD.Mercato.Notification       → Emails, notifications
└── SD.Mercato.Reports            → Analytics, reports
```

**Key Principle:** Modules are self-contained. Cross-module communication only via interfaces.

---

## 🔑 Key Entities Quick Reference

| Entity | Primary Key | Unique Constraints | Notes |
|--------|------------|-------------------|-------|
| User | UserID | Email | One role per user (MVP) |
| Store | StoreID | StoreName | One store per user (MVP) |
| Product | ProductID | SKU (per store) | Soft delete via Status |
| Category | CategoryID | Name | Max 3-level hierarchy |
| Cart | CartID | UserID | One active cart per user |
| Order | OrderID | OrderNumber | Marketplace-level |
| SubOrder | SubOrderID | SubOrderNumber | Seller-level |
| PaymentTransaction | TransactionID | - | Immutable audit log |
| Payout | PayoutID | - | Weekly batches |

---

## 🧪 Testing Strategy

### Unit Tests
- Domain logic in modules
- Business rule validation
- Calculation accuracy (payouts, commissions)

### Integration Tests
- API endpoints
- Database operations
- Module interactions

### Manual Testing Focus (MVP)
- End-to-end purchase flow
- Payment gateway integration
- Seller onboarding flow
- Order fulfillment workflow

**Note:** Test infrastructure not yet created. Follow existing patterns when adding tests.

---

## 🎯 MVP Constraints

**Single:**
- Currency (USD)
- Country/Region
- Language (English)
- Payment method (credit/debit card)

**Not Supported in MVP:**
- Guest checkout
- Product variants (each variant = separate product)
- Promotional codes
- Product reviews
- Seller messaging
- Mobile apps
- Multi-language/currency

---

## 📞 Getting Help

1. **Business questions**: Check `business-domain.md` TODOs
2. **Technical questions**: Review `architecture.md`
3. **Entity relationships**: See diagrams in `domain-model.md`
4. **Feature specs**: Refer to `prd.md`
5. **API design**: Check endpoint structure in `domain-model.md`

---

## ✅ Before You Start Coding

- [ ] Read Business Domain Documentation
- [ ] Review relevant domain model diagrams
- [ ] Understand module boundaries
- [ ] Check for open TODOs in your area
- [ ] Review existing module code structure
- [ ] Plan your entity models
- [ ] Design your API endpoints
- [ ] Consider validation rules
- [ ] Think about test cases

---

**Happy coding! 🚀**

*Last updated: 2025-11-20*
