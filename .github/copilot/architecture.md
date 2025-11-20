# Mercato – Architecture Overview

This document describes the high-level architecture of the Mercato system.  
It serves as a primary reference for developers, GitHub Copilot, and AI tools working on the project.

---

## 1. System Architecture Overview

Mercato is a modular monolith built on .NET.

1. The Web UI (AppUI project) handles interfaces for buyers, sellers, and administrators.
2. The Backend API exposes HTTP endpoints.
3. Domain modules located under `src/Modules` contain the business logic.
4. A SQL database persists all system data.

The UI communicates with the API through HTTP/JSON.  
The API communicates with modules through project references inside the same solution.

---

## 2. System Context (C4 Level 1)

The Mercato system interacts with:

1. Buyers (via browser or mobile device)
2. Sellers (via the Seller Panel)
3. Administrators (Admin Panel)
4. External payment providers (e.g., PayU, Stripe – TBD)
5. External shipping carriers (courier APIs – TBD)

Mercato acts as the central system orchestrating products, orders, payments, and shipping.

---

## 3. Containers (C4 Level 2)

| Container        | Technology          | Purpose                                                         |
|------------------|----------------------|------------------------------------------------------------------|
| Web UI           | .NET (Blazor/SPA)   | Presentation, routing, client-side validation, API communication |
| API              | .NET Web API        | HTTP endpoints, application logic, orchestration of modules      |
| Modules          | .NET Class Libraries| Domain logic, aggregates, business rules, domain services        |
| Database         | MS SQL Server + EF Core       | Data persistence, relational storage, transactions               |
| Background Jobs  | Hosted Services      | Async tasks: notifications, synchronization jobs                 |

The UI never talks directly to modules.  
All communication flows through the API.

---

## 4. Code Structure

Main paths:

| Path                                     | Description                                           |
|------------------------------------------|-------------------------------------------------------|
| `src/API/SD.Mercato.API`                 | Backend API host                                      |
| `src/AppUI/SD.Mercato.UI`                | Server-side UI                                        |
| `src/AppUI/SD.Mercato.UI.Client`         | Client-side web application                           |
| `src/Modules/SD.Mercato.*`               | Domain modules                                        |
| `src/Tests`                              | Unit and integration tests                            |

Each module is self-contained with its own models, services, handlers, and interfaces.

---

## 5. API Internal Layers

The API follows standard application layering:

1. **Presentation Layer**  
   Controllers or minimal API endpoints mapping incoming requests to use cases.

2. **Application Layer**  
   Command and query handlers (CQRS), business scenarios, orchestration across modules.

3. **Domain Layer (inside Modules)**  
   Entities, aggregates, value objects, domain services, business rules.

4. **Infrastructure Layer**  
   Repository implementations, integration clients (payments, shipping), logging, background tasks.

Dependency flow is strictly one-way:  
UI → API → Domain Modules → Infrastructure

---

## 6. Domain Modules

| Module Project                      | Responsibility                                                      |
|------------------------------------|---------------------------------------------------------------------|
| `SD.Mercato.Cart`                  | Cart, cart items, checkout process                                  |
| `SD.Mercato.History`               | Event history, order history, audit trail                           |
| `SD.Mercato.Notification`          | Email and in-app notifications                                      |
| `SD.Mercato.Payments`              | Payments, transactions, commissions, seller payouts                 |
| `SD.Mercato.ProductCatalog`        | Products, categories, attributes, catalog indexing                  |
| `SD.Mercato.Reports`               | Sales reports, commissions, platform KPIs                           |
| `SD.Mercato.SellerPanel`           | Shops, seller profiles, seller panel configuration                  |
| `SD.Mercato.Shipping`              | Shipping methods, price rules, carrier integrations                 |
| `SD.Mercato.Users`                 | Users, roles, authentication and authorization                      |

Each module must:

1. Expose public interfaces for other modules or API.
2. Hide internal domain models when possible.
3. Maintain its own DTOs for communication or API representation.

---

## 7. Communication Between Modules

Primary communication:

1. **Direct invocation** through C# interfaces injected via dependency injection.  
2. **Domain events** for cross-cutting processes (e.g., History, Notifications, Reports):

   - A module publishes a domain event.
   - Another module reacts and executes its own logic (e.g., sending an email, updating a report).

Domain events may be implemented via Mediator pattern or a lightweight event dispatcher.

---

## 8. API Design

API should follow modern REST principles:

1. Resource-based routing with proper nouns.
2. JSON response format.
3. Input validation on model level.
4. Versioning (e.g., `/api/v1/...`).

Typical domains for API endpoints:

- `/api/catalog/...`
- `/api/cart/...`
- `/api/orders/...`
- `/api/payments/...`
- `/api/sellers/...`
- `/api/users/...`
- `/api/reports/...`

---

## 9. Data Access

Recommended approach:

1. EF Core as ORM.
2. Each module contains its own repository implementations.
3. Database migrations kept either:
   - per module  
   - or in a separate shared migrations project.

Key rules:

- A module must not directly manipulate tables belonging to another module.  
- Shared data must be exposed through services or read models.

---

## 10. Security and Authorization

### 10.1 Authentication

The system uses **JWT (JSON Web Tokens)** for authentication:

1. **ASP.NET Core Identity** manages user accounts, passwords, and roles
2. **JWT tokens** are issued upon successful login (email/password or OAuth)
3. **OAuth 2.0** providers supported:
   - Google
   - Facebook
   - Apple (can be added with minimal effort)

### 10.2 Authorization

The system implements **role-based access control (RBAC)** with three primary roles:

1. **Buyer**
   - Can browse catalog
   - Can manage own cart
   - Can place orders
   - Can view own order history
   - Access only own data

2. **Seller**
   - Can manage own store
   - Can add/edit/delete products
   - Can view own orders
   - Can view own financial reports
   - Access only store-specific data
   - Each seller begins with a primary owner account
   - **Future**: Support for staff accounts with granular permissions

3. **Administrator**
   - Can manage the entire platform
   - Can access global configuration
   - Can view all reports
   - Full system access

### 10.3 Authorization Enforcement

Authorization is enforced at multiple levels:

1. **API Endpoint Level** - Using `[Authorize]` attributes and policies:
   ```csharp
   [Authorize(Policy = "RequireSellerRole")]
   public class SellerProductsController { }
   ```

2. **Domain Logic Level** - Business rules ensure data isolation:
   - Sellers can only access their own store data
   - Buyers can only access their own orders
   - Admins have unrestricted access

3. **Database Level** - Foreign keys and queries enforce data boundaries

### 10.4 JWT Token Structure

Tokens contain the following claims:
- User ID (nameid)
- Email
- Name
- Role
- Token ID (jti)
- Issuer and Audience
- Expiration (default: 60 minutes)

### 10.5 Seller Staff Extensibility

The database model supports future multi-user seller accounts:

- **SellerStaff** table links users to stores
- **IsOwner** flag identifies primary owner
- **JobTitle** and **IsActive** fields for staff management
- Designed for future granular permissions (CanManageProducts, CanProcessOrders, etc.)

When the staff account UI is built later, the backend requires minimal changes.

### 10.6 Security Best Practices

1. Passwords are hashed using **PBKDF2** (ASP.NET Core Identity default)
2. Password complexity requirements enforced (min 8 chars, uppercase, lowercase, digit)
3. Email uniqueness enforced at database level
4. JWT secret key must be changed in production
5. OAuth client secrets stored securely (User Secrets or Azure Key Vault)
6. CORS restricted to specific domains in production

### 10.7 Compliance

The system must comply with **GDPR** for personal data:
- User consent for data processing
- Right to access personal data
- Right to delete account
- Data portability

---

## 11. Logging, Monitoring, Audit

1. Central logging system (Application Insights).
2. Request correlation IDs.
3. Business events stored through the `History` module.
4. Application metrics for:
   - order volume
   - error rate
   - performance indicators

---

## 12. Testing Strategy

Test layers:

1. Unit tests for domain modules (`src/Tests`).
2. Integration tests for API using a test host and test database.
3. End-to-end tests (optional, future phase).

A feature is considered complete only when it includes proper module and API automated tests.

---

## 13. Future Architecture Evolution

As the system grows, the architecture may evolve:

1. Selected modules may be extracted into microservices  
   (e.g., Payments, Notifications).
2. Event queues (Kafka, Azure Service Bus) can replace in-process events.
3. The single database may be split into:
   - separate schemas per module  

The modular monolith approach ensures the system is ready for gradual decomposition.
