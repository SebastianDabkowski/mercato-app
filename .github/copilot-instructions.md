# Mercato - GitHub Copilot Instructions

## Project Overview

Mercato is a multi-vendor e-commerce marketplace platform that connects independent online stores. Sellers can register their shops, list products, manage orders and payouts, while buyers can browse the global catalog, place orders and track their purchases.

**Project Type:** Modular monolith web application  
**Current Phase:** MVP Development  
**Architecture Pattern:** CQRS, Domain-Driven Design, Modular Monolith

## Tech Stack

### Backend
- **.NET 9.0** - Primary backend framework
- **ASP.NET Core Web API** - RESTful API endpoints
- **Entity Framework Core** - ORM and database access
- **MS SQL Server** - Primary database

### Frontend
- **Blazor WebAssembly** - Client-side SPA framework
- **ASP.NET Core** - Server-side hosting

### Key Libraries & Patterns
- CQRS pattern for commands and queries
- Repository pattern for data access
- Domain-driven design for business logic
- Modular architecture with clear boundaries

## Project Structure

```
src/
├── API/
│   └── SD.Mercato.API/              # Backend HTTP API
├── AppUI/
│   ├── SD.Mercato.UI/               # Server-side UI hosting
│   └── SD.Mercato.UI.Client/        # Blazor WebAssembly client
├── Modules/                         # Domain modules (self-contained)
│   ├── SD.Mercato.Users/            # Authentication, users, roles
│   ├── SD.Mercato.SellerPanel/      # Store profiles, seller settings
│   ├── SD.Mercato.ProductCatalog/   # Products, categories, search
│   ├── SD.Mercato.Cart/             # Shopping cart, checkout
│   ├── SD.Mercato.Payments/         # Transactions, payouts, gateway
│   ├── SD.Mercato.History/          # Orders, SubOrders, history
│   ├── SD.Mercato.Shipping/         # Shipping methods, tracking
│   ├── SD.Mercato.Notification/     # Emails, notifications
│   └── SD.Mercato.Reports/          # Analytics, reports
└── Tests/                           # Unit and integration tests (TBD)
```

## Coding Guidelines

### General Principles
1. **Minimal Changes**: Make the smallest possible changes to achieve the goal
2. **Module Isolation**: Modules are self-contained; cross-module communication only via interfaces
3. **Dependency Flow**: UI → API → Domain Modules → Infrastructure (one-way only)
4. **No Direct Module Access**: UI never talks directly to modules; all communication flows through the API

### .NET & C# Standards
- Use **C# 13** features with .NET 9.0
- Enable **nullable reference types** (`<Nullable>enable</Nullable>`)
- Use **implicit usings** (`<ImplicitUsings>enable</ImplicitUsings>`)
- Follow standard .NET naming conventions (PascalCase for public members, camelCase for private)
- Prefer `async/await` for I/O operations
- Use `record` types for immutable DTOs and value objects
- Use `required` keyword for mandatory properties in C# 11+

### Code Organization
- One public class per file
- File name matches the class name
- Organize using statements (System first, then third-party, then project)
- Keep methods focused and small (< 20 lines when possible)

### Domain-Driven Design
- **Entities**: Use identity-based equality (e.g., `ProductID`, `OrderID`)
- **Value Objects**: Use structural equality (e.g., `Money`, `Address`)
- **Aggregates**: Enforce business rules and invariants
- **Domain Services**: For operations that don't belong to a single entity
- **Repositories**: Abstract data access per aggregate root

### API Design
- RESTful endpoints following standard conventions
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
- Return proper status codes (200, 201, 400, 404, 500, etc.)
- Use DTOs for request/response models (never expose domain entities directly)
- Validate inputs at the API boundary
- Handle exceptions with global error handling middleware

### Database & EF Core
- Use Entity Framework Core for all database access
- Define entities with proper constraints and relationships
- Use migrations for schema changes (`dotnet ef migrations add`)
- Never expose `DbContext` outside the infrastructure layer
- Use repository pattern to abstract data access
- Apply soft deletes where appropriate (use `Status` field)

### Testing Approach
- Write unit tests for domain logic and business rules
- Write integration tests for API endpoints and database operations
- Test validation rules and business constraints
- Test payment calculations and commission logic
- Follow existing test patterns when adding new tests
- **Note**: Test infrastructure is not yet fully established; follow patterns as they emerge

### Error Handling
- Use exceptions for exceptional cases, not control flow
- Provide meaningful error messages
- Log errors with sufficient context
- Return user-friendly messages to clients
- Never expose internal implementation details in error responses

### Comments & Documentation
- Write self-documenting code with clear naming
- Add XML documentation comments for public APIs
- Document complex business rules with inline comments
- Keep comments up to date with code changes
- Don't state the obvious - comment the "why", not the "what"

## Business Rules (Quick Reference)

### Products
- Price must be > $0
- SKU unique per store
- At least 1 image required
- Stock cannot be negative
- Soft delete via Status field

### Cart
- Max 50 unique items per cart
- Cart expires after 30 days of inactivity
- One active cart per user
- Validate stock availability at checkout

### Orders
- Created only after successful payment
- Stock deducted at order creation
- One Order can have multiple SubOrders (one per seller)
- Order states: Pending → Processing → Shipped → Delivered → Completed

### Payments & Payouts
- **Platform commission**: 15% (on products only, not shipping)
- **Processing fee**: 2.9% + $0.30 per transaction
- Payout after delivery confirmation (or auto-confirm after 14 days)
- **Minimum payout**: $10
- **Payout frequency**: Weekly (Mondays)

**Payout Calculation Example:**
```
Order Total: $100 (products) + $10 (shipping) = $110
Commission: $100 × 15% = $15
Processing Fee: $110 × 2.9% + $0.30 = $3.49
Seller Receives: $110 - $15 - $3.49 = $91.51
```

### Shipping
- Seller must ship within 3 business days
- Tracking number is recommended
- Auto-delivery confirmation after 14 days without dispute

## MVP Constraints

**Single Support Only:**
- Currency: USD
- Country/Region: United States
- Language: English
- Payment Method: Credit/Debit Card

**Not Supported in MVP:**
- Guest checkout
- Product variants (each variant = separate product)
- Promotional codes/discounts
- Product reviews and ratings
- Seller-buyer messaging
- Mobile applications
- Multi-language or multi-currency support
- Returns/refunds (future phase)

## Key Entities

| Entity | Primary Key | Unique Constraints | Notes |
|--------|-------------|-------------------|-------|
| User | UserID | Email | One role per user (MVP) |
| Store | StoreID | StoreName | One store per seller (MVP) |
| Product | ProductID | SKU (per store) | Soft delete via Status |
| Category | CategoryID | Name | Max 3-level hierarchy |
| Cart | CartID | UserID | One active cart per user |
| Order | OrderID | OrderNumber | Marketplace-level order |
| SubOrder | SubOrderID | SubOrderNumber | Seller-level order |
| PaymentTransaction | TransactionID | - | Immutable audit log |
| Payout | PayoutID | - | Weekly batch processing |

## Documentation Resources

Comprehensive documentation is available in `.github/copilot/`:

- **[Quick Start Guide](.github/copilot/quickstart.md)** - Start here for project overview and documentation roadmap
- **[Business Domain Documentation](.github/copilot/business-domain.md)** - Core business flows, entities, rules, and MVP scope
- **[Domain Model Reference](.github/copilot/domain-model.md)** - Entity relationships, state machines, and data flows
- **[Implementation Status](.github/copilot/implementation-status.md)** - Feature tracking and progress monitoring
- **[Product Requirements (PRD)](.github/copilot/prd.md)** - Product requirements and feature specifications
- **[Architecture Overview](.github/copilot/architecture.md)** - Technical architecture and design decisions

**Always refer to these documents when:**
- Implementing new features
- Understanding business requirements
- Designing API endpoints
- Modeling domain entities
- Checking implementation status

## Development Workflow

### Before Starting Work
1. Review relevant documentation in `.github/copilot/`
2. Understand module boundaries and dependencies
3. Check for open TODOs in the business domain docs
4. Plan entity models and API endpoints
5. Consider validation rules and edge cases

### Building & Testing
```bash
# Restore dependencies
dotnet restore SD.Mercato.sln

# Build solution
dotnet build SD.Mercato.sln

# Run tests (when available)
dotnet test SD.Mercato.sln

# Run API
dotnet run --project src/API/SD.Mercato.API

# Run UI
dotnet run --project src/AppUI/SD.Mercato.UI
```

### Code Changes
1. Make minimal, focused changes
2. Follow existing code patterns
3. Maintain module boundaries
4. Add/update tests for business logic
5. Validate inputs at API boundaries
6. Document complex business rules

## Common Patterns

### CQRS Command Example
```csharp
public record CreateProductCommand(
    Guid StoreId,
    string Name,
    decimal Price,
    string SKU
);

public class CreateProductCommandHandler
{
    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command)
    {
        // Validate
        // Create entity
        // Save to repository
        // Return result
    }
}
```

### Repository Pattern Example
```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetByStoreIdAsync(Guid storeId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
}
```

### API Controller Example
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(...);
        var result = await _handler.HandleAsync(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
        
        return BadRequest(result.Error);
    }
}
```

## Security Considerations

- **Authentication**: Required for all non-public endpoints
- **Authorization**: Role-based access control (Buyer, Seller, Admin)
- **Input Validation**: Validate all user inputs at API boundary
- **SQL Injection**: Use parameterized queries (EF Core handles this)
- **XSS Prevention**: Encode outputs in views
- **Secrets Management**: Never commit secrets; use user secrets or environment variables
- **Payment Security**: Follow PCI compliance guidelines
- **Audit Trail**: Log all financial transactions immutably

## Important TODOs (Requires Business Decision)

When implementing features in these areas, check the business domain documentation for open questions:

1. Seller approval process (auto-approve vs. admin review)
2. Bank account details requirements (varies by country)
3. Minimum product price threshold
4. Restricted product categories
5. Stock locking in cart vs. checkout notification
6. Acceptable price change threshold during checkout
7. Partial order fulfillment support
8. Late shipment penalties
9. Returns/refunds fee handling

**Action**: Review TODOs in business-domain.md before implementing these features.

## Performance Considerations

- Use async/await for I/O operations
- Implement pagination for large result sets
- Cache frequently accessed data when appropriate
- Optimize database queries (use indexes, avoid N+1 queries)
- Use efficient JSON serialization
- Consider background jobs for long-running operations

## Accessibility & UX

- Follow WCAG 2.1 guidelines for web accessibility
- Provide meaningful error messages
- Use loading indicators for async operations
- Implement proper form validation with user feedback
- Ensure responsive design for different screen sizes

## Version Control

- Keep commits focused and atomic
- Write clear, descriptive commit messages
- Reference issue numbers in commits when applicable
- Don't commit build artifacts or dependencies
- Use `.gitignore` to exclude temporary files

---

**Last Updated**: 2025-11-20  
**Document Version**: 1.0  
**Target Audience**: GitHub Copilot, AI coding assistants, and developers
