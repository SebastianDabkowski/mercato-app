# Mercato – Product Requirements Document (PRD)

Mercato is a multi-vendor marketplace allowing independent online stores to list, manage and sell products in one shared platform. The system consists of a Seller Panel and a Buyer Portal, backed by a modular .NET backend and a modern web UI.

---

## 1. Goal of the system

Create a scalable marketplace platform enabling sellers to onboard their shops, manage offers and fulfill orders, while buyers browse the global catalog, place orders and track deliveries.  
The platform must support multiple sellers, each with isolated data and their own administration area.

---

## 2. Main Users

### Buyers
- Browse the catalog
- Add items to cart
- Place orders and pay
- Track order status
- Manage their account

### Sellers
- Register shop and configure settings
- Add and manage products
- Handle orders
- Manage returns/refunds
- View sales metrics

### Administrators
- Manage platform configuration
- Review seller applications
- Moderate catalog, categories, reports
- View global analytics

---

## 3. Core Features

### 3.1 Product Catalog
- Create/update products (title, description, images, stock, SKU, price)
- Categories and attributes
- Search and filtering
- Product visibility control per seller

### 3.2 Shopping Cart & Checkout
- Add/remove items
- Multi-seller cart support
- Shipping selection per seller
- Payment integration (PayU/Stripe – TBD)
- Order confirmation flow

### 3.3 Payments
- Create transaction for each seller’s portion
- Track payment status
- Initiate payouts to sellers
- Payment logs and audit trail

### 3.4 Orders
- Unified order number (marketplace-level)
- Seller-level sub-orders
- Status flow: Created → Paid → Processing → Shipped → Delivered
- Return/refund handling
- Delivery tracking integration (TBD)

### 3.5 Seller Panel
- Product management panel
- Stock management
- Order management
- Payout overview
- Seller onboarding wizard
- Shop profile settings (name, logo, contact)

### 3.6 Notifications
- Email + in-system notifications
- Events: new order, status change, refund request, payout completed

### 3.7 Reports
- Sales per seller
- Commissions
- Traffic and conversion data (seller & admin)
- Operational KPIs

---

## 4. Non-Functional Requirements

### Performance
- Handle min. 500 concurrent users in MVP
- Response time < 300ms for core endpoints

### Security
- Role-based access control (Buyer, Seller, Admin)
- Isolated seller data (strict multi-tenancy inside one DB)
- GDPR-ready user management

### Architecture
- Modular monolith
- Clean boundaries between modules:
  - Cart
  - Payments
  - ProductCatalog
  - Users
  - SellerPanel
  - Shipping
  - Notifications
  - Reports
  - History

### Scalability
- Modules must be separable into microservices in the future
- API-first approach

### Technology
- Backend: .NET 8
- Frontend: Blazor or Angular (depending on UI project)
- Database: SQL
- Hosting: Azure App Service
- CI/CD: GitHub Actions

---

## 5. Success Criteria

### Business
- First 50 sellers onboarded
- At least 5K SKUs in catalog
- At least 300 monthly active buyers within 3 months

### Product
- Stable checkout process with <2% failure rate
- <1% refund/rejection rate due to technical issues
- Seller onboarding < 5 minutes

---

## 6. Out of Scope (MVP)

- Advanced promotions and discount engine
- Recommendation engine
- AI-based content moderation
- Multi-language support
- Multi-currency pricing
- Mobile application

---

## 7. Modules Mapping (to the solution structure)

| Module                        | Folder                                      |
|-------------------------------|---------------------------------------------|
| Cart                          | `src/Modules/SD.Mercato.Cart`              |
| History                       | `src/Modules/SD.Mercato.History`           |
| Notification                  | `src/Modules/SD.Mercato.Notification`      |
| Payments                      | `src/Modules/SD.Mercato.Payments`          |
| ProductCatalog                | `src/Modules/SD.Mercato.ProductCatalog`    |
| Reports                       | `src/Modules/SD.Mercato.Reports`           |
| SellerPanel                   | `src/Modules/SD.Mercato.SellerPanel`       |
| Shipping                      | `src/Modules/SD.Mercato.Shipping`          |
| Users                         | `src/Modules/SD.Mercato.Users`             |

---

## 8. Definition of Ready (DoR)
A feature is ready when:
- Accepted by Product Owner
- Has clear acceptance criteria
- Has API inputs/outputs defined
- Has data model defined
- Has no unresolved dependencies

## 9. Definition of Done (DoD)
A feature is done when:
- Implemented
- Tested (unit + integration)
- API documented
- UI validated manually
- Logged in release notes
