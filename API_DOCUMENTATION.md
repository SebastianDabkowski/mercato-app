# Authentication & Authorization API Documentation

## Overview

The Mercato platform implements a complete authentication and authorization system using ASP.NET Core Identity, JWT tokens, and OAuth 2.0 for social logins.

## Authentication Flow

### Email/Password Registration & Login

```
┌─────────┐                    ┌─────────┐                    ┌──────────┐
│ Client  │                    │   API   │                    │ Database │
└────┬────┘                    └────┬────┘                    └────┬─────┘
     │                              │                              │
     │ POST /api/auth/register      │                              │
     │─────────────────────────────>│                              │
     │                              │  Create User & Hash Password │
     │                              │─────────────────────────────>│
     │                              │<─────────────────────────────│
     │                              │  Assign Role                 │
     │                              │─────────────────────────────>│
     │                              │<─────────────────────────────│
     │  JWT Token + User Info       │  Generate JWT Token          │
     │<─────────────────────────────│                              │
     │                              │                              │
     │ Store Token in LocalStorage  │                              │
     │                              │                              │
     │ POST /api/auth/login         │                              │
     │─────────────────────────────>│                              │
     │                              │  Verify Credentials          │
     │                              │─────────────────────────────>│
     │                              │<─────────────────────────────│
     │  JWT Token + User Info       │  Generate JWT Token          │
     │<─────────────────────────────│                              │
     │                              │                              │
```

### Social Login (Google/Facebook)

```
┌─────────┐         ┌──────────┐         ┌─────────┐         ┌──────────┐
│ Client  │         │   API    │         │  OAuth  │         │ Database │
└────┬────┘         └────┬─────┘         │Provider │         └────┬─────┘
     │                   │                └────┬────┘              │
     │ Click "Login with Google"              │                   │
     │                   │                     │                   │
     │ Redirect to OAuth Provider             │                   │
     │───────────────────────────────────────>│                   │
     │                   │                     │                   │
     │ User authenticates & approves          │                   │
     │                   │                     │                   │
     │ Redirect with token                    │                   │
     │<───────────────────────────────────────│                   │
     │                   │                     │                   │
     │ POST /api/auth/external-login          │                   │
     │──────────────────>│                     │                   │
     │                   │  Create/Find User   │                   │
     │                   │────────────────────────────────────────>│
     │                   │<────────────────────────────────────────│
     │  JWT Token        │  Generate JWT Token │                   │
     │<──────────────────│                     │                   │
```

## API Endpoints

### Authentication Endpoints

#### POST /api/auth/register
Register a new user with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "role": "Buyer"
}
```

**Validation:**
- Email: Required, valid email format, unique
- Password: Min 8 chars, 1 uppercase, 1 lowercase, 1 digit
- FirstName: Required, max 100 chars
- LastName: Required, max 100 chars
- PhoneNumber: Optional, valid phone format
- Role: Required, must be "Buyer", "Seller", or "Administrator"

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Registration successful",
  "user": {
    "id": "abc123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "role": "Buyer",
    "isEmailVerified": true
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "User with this email already exists",
  "token": null,
  "user": null
}
```

---

#### POST /api/auth/login
Login with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Login successful",
  "user": {
    "id": "abc123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "role": "Buyer",
    "isEmailVerified": true,
    "lastLoginAt": "2025-11-20T10:30:00Z"
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid email or password",
  "token": null,
  "user": null
}
```

**Error Response (Account Locked):**
```json
{
  "success": false,
  "message": "Account is locked due to multiple failed login attempts. Please try again later.",
  "token": null,
  "user": null
}
```

**Note:** After 5 failed login attempts, the account is locked for 15 minutes to prevent brute-force attacks.

---

#### POST /api/auth/external-login
Login or register with external OAuth provider (Google, Facebook).

**Request:**
```json
{
  "provider": "Google",
  "token": "oauth_token_from_provider",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "role": "Buyer",
  "externalProviderId": "google_user_id_123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Login successful",
  "user": {
    "id": "abc123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Buyer",
    "isEmailVerified": true,
    "externalProvider": "Google"
  }
}
```

---

#### POST /api/auth/logout
Logout the current user.

**Authorization:** Bearer Token Required

**Request:** Empty

**Response (200 OK):**
```json
{
  "message": "Logout successful"
}
```

---

### User Management Endpoints

#### GET /api/users/profile
Get the current user's profile.

**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "abc123",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "role": "Buyer",
  "isEmailVerified": true,
  "createdAt": "2025-11-01T10:00:00Z",
  "lastLoginAt": "2025-11-20T10:30:00Z"
}
```

---

#### PUT /api/users/profile
Update the current user's profile.

**Authorization:** Bearer Token Required

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890"
}
```

**Response (200 OK):**
```json
{
  "message": "Profile updated successfully"
}
```

---

## Authorization

### Roles

The system supports three primary roles:

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
   - Future: Support for staff accounts with limited permissions

3. **Administrator**
   - Can manage the entire platform
   - Can access global configuration
   - Can view all reports
   - Full system access

### Authorization Policies

The following policies are defined:

- `RequireBuyerRole` - Requires Buyer role
- `RequireSellerRole` - Requires Seller role
- `RequireAdministratorRole` - Requires Administrator role

### Using Authorization in Controllers

```csharp
[Authorize(Policy = "RequireSellerRole")]
public class SellerProductsController : ControllerBase
{
    // Only sellers can access these endpoints
}

[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    // Only administrators can access these endpoints
}
```

### JWT Token Structure

The JWT token contains the following claims:

```json
{
  "nameid": "user_id_abc123",
  "email": "user@example.com",
  "unique_name": "John Doe",
  "role": "Buyer",
  "jti": "unique_token_id",
  "nbf": 1700472600,
  "exp": 1700476200,
  "iat": 1700472600,
  "iss": "MercatoAPI",
  "aud": "MercatoClient"
}
```

### Token Expiry

- Default expiry: 60 minutes
- Configurable via `JwtSettings:ExpiryInMinutes` in appsettings.json
- No refresh token implemented in MVP (can be added later)

---

## Seller Staff Extensibility

The system is designed to support multiple staff accounts per seller in the future.

### Database Model

The `SellerStaff` entity supports:
- Multiple users associated with a single seller/store
- Owner designation (IsOwner flag)
- Job titles
- Active/inactive status
- Timestamps for audit trail

### Future Enhancements

When staff account UI is built, the following can be added without backend refactoring:

1. **Permissions** (add to SellerStaff model):
   - CanManageProducts
   - CanProcessOrders
   - CanViewReports
   - CanManageStaff
   - CanEditStoreProfile

2. **API Endpoints** (already supported by data model):
   - POST /api/sellers/staff - Add staff member
   - GET /api/sellers/staff - List staff
   - PUT /api/sellers/staff/{id} - Update staff
   - DELETE /api/sellers/staff/{id} - Remove staff

---

## Security Considerations

### Password Security
- Passwords are hashed using ASP.NET Core Identity's default hashing (PBKDF2)
- Minimum complexity requirements enforced
- No password reset flow in MVP (TODO)

### JWT Security
- Tokens are signed with HMAC SHA256
- Secret key must be changed in production
- Tokens are stateless (no server-side storage)
- Client stores tokens in LocalStorage

### OAuth Security
- OAuth tokens should be verified with provider APIs (TODO in production)
- Client secrets must be kept secure
- Use User Secrets or Key Vault in production

### CORS
- Currently allows all origins in development
- Must be restricted to specific domains in production

### TODO Items

The following security-related items need clarification:

1. Should inactive users be able to log in, or return 403 Forbidden?
2. Should seller approval be automatic or require admin review?
3. What are password reset requirements?
4. Should we implement refresh tokens?
5. Should we implement account lockout after failed attempts?

---

## Testing

### Using the API

1. **Register a new user:**
```bash
curl -X POST https://localhost:7147/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }'
```

2. **Login:**
```bash
curl -X POST https://localhost:7147/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

3. **Get profile (with token):**
```bash
curl -X GET https://localhost:7147/api/users/profile \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Frontend Pages

- `/register` - Registration page
- `/login` - Login page
- `/profile` - User profile page (requires authentication)

---

## Error Codes

| Status Code | Meaning |
|-------------|---------|
| 200 | Success |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (invalid credentials or missing token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |
