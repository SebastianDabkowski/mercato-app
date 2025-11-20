# Mercato – Marketplace Platform

Mercato is a multi-vendor e-commerce platform that connects independent online stores in a single marketplace.  
Sellers can register their shops, list products, manage orders and payouts, while buyers can browse the global catalog, place orders and track their purchases.

---

## Solution overview

The solution is built as a modular .NET application:

1. Backend HTTP API for core marketplace operations.
2. Web application (AppUI) for end-user and back-office interfaces.
3. Domain modules grouped in `Modules` and referenced by API/UI.
4. Test projects validating business logic and integrations.

The design follows a modular-monolith approach: each business area lives in a separate project, but everything is composed into one deployable application.

---

## Repository structure

Top-level layout:

| Path            | Description                                      |
|-----------------|--------------------------------------------------|
| `.github`       | CI/CD, GitHub Actions and repo automation        |
| `docs`          | Architecture and project documentation           |
| `src`           | Application source code                          |
| `src/API`       | Backend HTTP API                                 |
| `src/AppUI`     | Web application projects                         |
| `src/Modules`   | Domain modules (Cart, Catalog, Payments, etc.)   |
| `src/Tests`     | Automated tests                                  |
| `SD.Mercato.sln`| Main Visual Studio / `dotnet` solution file      |

---

## Projects

### API

| Project path            | Description                                      |
|-------------------------|--------------------------------------------------|
| `src/API/SD.Mercato.API` | Main backend API exposing marketplace endpoints (catalog, cart, orders, users, payments). |

### Web UI

| Project path                            | Description                                            |
|----------------------------------------|--------------------------------------------------------|
| `src/AppUI/SD.Mercato.UI`              | Server-side part of the web application (hosting, routing, composition). |
| `src/AppUI/SD.Mercato.UI.Client`       | Client-side UI project (frontend for buyers and sellers). |

### Domain modules

All domain logic is split into feature modules under `src/Modules`:

| Project path                             | Responsibility (high level)                                      |
|-----------------------------------------|------------------------------------------------------------------|
| `src/Modules/SD.Mercato.Cart`          | Shopping cart and checkout flow.                                |
| `src/Modules/SD.Mercato.History`       | Order history and audit trail.                                  |
| `src/Modules/SD.Mercato.Notification`  | E-mails, in-app notifications and background messages.          |
| `src/Modules/SD.Mercato.Payments`      | Payment methods, transactions and payout logic.                 |
| `src/Modules/SD.Mercato.ProductCatalog`| Product catalog, categories, search and listing.                |
| `src/Modules/SD.Mercato.Reports`       | Basic reporting and analytics for admins and sellers.           |
| `src/Modules/SD.Mercato.SellerPanel`   | Seller onboarding and back-office (shop profile, offers, stats).|
| `src/Modules/SD.Mercato.Shipping`      | Shipping options, tracking data, integration points.            |
| `src/Modules/SD.Mercato.Users`         | Users, roles, authentication and authorization.                 |

### Tests

`src/Tests` contains automated tests (unit, integration or end-to-end), structured per module or vertical slice.

---

## Requirements

1. .NET SDK (version defined in the solution or `global.json`).
2. A supported database engine and connection string configured in API settings.
3. NodeJS / frontend toolchain if the UI project requires it (check `package.json` inside `AppUI`).

---

## Getting started

1. Clone the repository.
2. Restore and build the solution:

   ```bash
   dotnet restore SD.Mercato.sln
   dotnet build SD.Mercato.sln
