# GDPR API Usage Examples

This document provides practical examples for using Mercato's GDPR compliance API endpoints.

## Table of Contents

1. [User Consent Management](#user-consent-management)
2. [Data Export (Portability)](#data-export-portability)
3. [Account Deletion](#account-deletion)
4. [Admin Privacy Tools](#admin-privacy-tools)

---

## User Consent Management

### Get Current Consent Settings

**Endpoint:** `GET /api/gdpr/consent`

**Authentication:** Required (Bearer token)

**Request:**
```bash
curl -X GET https://api.mercato.com/api/gdpr/consent \
  -H "Authorization: Bearer {your_access_token}"
```

**Response:**
```json
{
  "emailMarketingConsent": true,
  "emailMarketingConsentUpdatedAt": "2024-11-20T10:30:00Z"
}
```

---

### Update Email Marketing Consent

**Endpoint:** `PUT /api/gdpr/consent/email-marketing`

**Authentication:** Required (Bearer token)

**Request:**
```bash
curl -X PUT https://api.mercato.com/api/gdpr/consent/email-marketing \
  -H "Authorization: Bearer {your_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "emailMarketingConsent": false
  }'
```

**Response:**
```json
{
  "emailMarketingConsent": false,
  "emailMarketingConsentUpdatedAt": "2024-11-23T11:15:00Z"
}
```

**Use Cases:**
- User wants to opt out of marketing emails
- User wants to opt in to newsletter
- GDPR compliance - right to withdraw consent (Article 7)

---

## Data Export (Portability)

### Export User Data in JSON Format

**Endpoint:** `POST /api/gdpr/export`

**Authentication:** Required (Bearer token)

**Request:**
```bash
curl -X POST https://api.mercato.com/api/gdpr/export \
  -H "Authorization: Bearer {your_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "format": "json"
  }'
```

**Response:** Downloads a JSON file named `user_data_20241123_111500.json`

**Sample JSON Output:**
```json
{
  "data": {
    "profile": {
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "+1-555-0123",
      "createdAt": "2024-01-15T09:00:00Z"
    },
    "orders": [
      {
        "orderNumber": "MKT-2024-000123",
        "orderDate": "2024-11-20T14:30:00Z",
        "totalAmount": 125.50,
        "currency": "USD",
        "status": "Completed",
        "paymentStatus": "Paid",
        "deliveryAddress": {
          "recipientName": "John Doe",
          "addressLine1": "123 Main St",
          "addressLine2": "Apt 4B",
          "city": "New York",
          "state": "NY",
          "postalCode": "10001",
          "country": "United States"
        },
        "items": [
          {
            "productName": "Wireless Mouse",
            "storeName": "Tech Store",
            "quantity": 1,
            "price": 25.00,
            "subtotal": 25.00
          },
          {
            "productName": "USB-C Cable",
            "storeName": "Tech Store",
            "quantity": 2,
            "price": 15.00,
            "subtotal": 30.00
          }
        ]
      }
    ],
    "consent": {
      "emailMarketingConsent": true,
      "emailMarketingConsentUpdatedAt": "2024-01-15T09:00:00Z"
    },
    "account": {
      "createdAt": "2024-01-15T09:00:00Z",
      "lastLoginAt": "2024-11-23T10:00:00Z",
      "isEmailVerified": true,
      "externalProvider": null
    }
  },
  "format": "json",
  "exportedAt": "2024-11-23T11:15:00Z"
}
```

---

### Export User Data in CSV Format

**Endpoint:** `POST /api/gdpr/export`

**Authentication:** Required (Bearer token)

**Request:**
```bash
curl -X POST https://api.mercato.com/api/gdpr/export \
  -H "Authorization: Bearer {your_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "format": "csv"
  }'
```

**Response:** Downloads a CSV file named `user_data_20241123_111500.csv`

**Sample CSV Output:**
```csv
PROFILE DATA
Field,Value
Email,"john.doe@example.com"
First Name,"John"
Last Name,"Doe"
Phone Number,"+1-555-0123"
Account Created,"2024-01-15 09:00:00"

CONSENT DATA
Field,Value
Email Marketing Consent,True
Consent Updated At,"2024-01-15 09:00:00"

ACCOUNT DATA
Field,Value
Created At,"2024-01-15 09:00:00"
Last Login,"2024-11-23 10:00:00"
Email Verified,True
External Provider,"N/A"

ORDER HISTORY
Order Number,Order Date,Total Amount,Currency,Status,Payment Status
"MKT-2024-000123","2024-11-20 14:30:00",125.50,"USD","Completed","Paid"
```

**Use Cases:**
- User requests a copy of their data
- GDPR compliance - right to data portability (Article 20)
- User wants to migrate to another platform
- User wants to keep personal records

---

## Account Deletion

### Delete User Account

**Endpoint:** `DELETE /api/gdpr/account`

**Authentication:** Required (Bearer token)

**Request:**
```bash
curl -X DELETE https://api.mercato.com/api/gdpr/account \
  -H "Authorization: Bearer {your_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "confirmationEmail": "john.doe@example.com",
    "reason": "No longer using the service"
  }'
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Account successfully deleted. Personal data has been anonymized.",
  "deletedAt": "2024-11-23T11:30:00Z"
}
```

**Response (Error - Email Mismatch):**
```json
{
  "success": false,
  "message": "Confirmation email does not match",
  "deletedAt": null
}
```

**What Happens:**
1. User account is marked as deleted (`IsDeleted = true`)
2. Personal data is anonymized:
   - Name → `[DELETED]`
   - Email → `deleted_{userId}@anonymized.mercato`
   - Phone → Removed
   - Marketing consent → Revoked
3. User can no longer log in
4. Order history is retained in anonymized form (legal requirement)

**Important Notes:**
- User must confirm deletion by entering their exact email address
- Action is **irreversible**
- Historical orders are retained for 7 years (legal/accounting requirements)
- Buyer information in orders is pseudonymized but retained

**Use Cases:**
- User wants to close their account
- GDPR compliance - right to erasure (Article 17)
- User no longer needs the service

---

## Admin Privacy Tools

### Get User Data for GDPR Audit

**Endpoint:** `POST /api/admin/gdpr/user-data`

**Authentication:** Required (Bearer token with Administrator role)

**Request (Search by Email):**
```bash
curl -X POST https://api.mercato.com/api/admin/gdpr/user-data \
  -H "Authorization: Bearer {admin_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com"
  }'
```

**Request (Search by User ID):**
```bash
curl -X POST https://api.mercato.com/api/admin/gdpr/user-data \
  -H "Authorization: Bearer {admin_access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  }'
```

**Response:**
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1-555-0123",
    "role": "Buyer",
    "createdAt": "2024-01-15T09:00:00Z",
    "lastLoginAt": "2024-11-23T10:00:00Z",
    "isDeleted": false,
    "deletedAt": null,
    "isEmailVerified": true,
    "externalProvider": null
  },
  "orders": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "orderNumber": "MKT-2024-000123",
      "createdAt": "2024-11-20T14:30:00Z",
      "totalAmount": 125.50,
      "status": "Completed",
      "paymentStatus": "Paid",
      "buyerEmail": "john.doe@example.com",
      "buyerPhone": "+1-555-0123",
      "deliveryAddress": "123 Main St, Apt 4B, New York, NY 10001"
    }
  ],
  "notifications": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440002",
      "notificationType": "Email",
      "eventType": "OrderCreated",
      "recipientEmail": "john.doe@example.com",
      "createdAt": "2024-11-20T14:30:00Z",
      "status": "Sent"
    }
  ],
  "consent": {
    "emailMarketingConsent": true,
    "emailMarketingConsentUpdatedAt": "2024-01-15T09:00:00Z"
  }
}
```

**Use Cases:**
- Responding to Data Subject Access Requests (DSAR)
- Privacy officer investigating compliance issues
- Auditing user data for GDPR compliance
- Verifying data retention policies

**Important Notes:**
- This endpoint is **only** accessible to administrators
- All access is logged in the audit log
- Includes admin user ID, email, timestamp, and IP address
- Should be used only for legitimate compliance purposes

**Audit Log Entry Created:**
```json
{
  "adminUserId": "admin_user_id",
  "adminEmail": "admin@mercato.com",
  "action": "GDPR_DATA_ACCESS",
  "entityType": "User",
  "entityId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "Admin accessed GDPR data for user john.doe@example.com",
  "ipAddress": "192.168.1.100",
  "performedAt": "2024-11-23T11:45:00Z"
}
```

---

## Error Responses

### Authentication Required
```json
{
  "status": 401,
  "message": "Unauthorized"
}
```

### Invalid Request
```json
{
  "status": 400,
  "message": "Invalid format. Supported formats: json, csv"
}
```

### User Not Found
```json
{
  "status": 404,
  "message": "User not found"
}
```

### Insufficient Permissions
```json
{
  "status": 403,
  "message": "Access denied. Administrator role required."
}
```

---

## Integration Examples

### JavaScript / TypeScript

```typescript
// Get user consent
async function getUserConsent(accessToken: string) {
  const response = await fetch('https://api.mercato.com/api/gdpr/consent', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  });
  return await response.json();
}

// Update marketing consent
async function updateMarketingConsent(accessToken: string, consent: boolean) {
  const response = await fetch('https://api.mercato.com/api/gdpr/consent/email-marketing', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ emailMarketingConsent: consent })
  });
  return await response.json();
}

// Export user data
async function exportUserData(accessToken: string, format: 'json' | 'csv') {
  const response = await fetch('https://api.mercato.com/api/gdpr/export', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ format })
  });
  
  // Download the file
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `user_data_${Date.now()}.${format}`;
  document.body.appendChild(a);
  a.click();
  a.remove();
}

// Delete account
async function deleteAccount(accessToken: string, email: string, reason?: string) {
  const response = await fetch('https://api.mercato.com/api/gdpr/account', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      confirmationEmail: email,
      reason
    })
  });
  return await response.json();
}
```

### C# / .NET

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class GdprApiClient
{
    private readonly HttpClient _httpClient;

    public GdprApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ConsentResponse> GetConsentAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        return await _httpClient.GetFromJsonAsync<ConsentResponse>(
            "https://api.mercato.com/api/gdpr/consent");
    }

    public async Task<ConsentResponse> UpdateMarketingConsentAsync(
        string accessToken, bool consent)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        var request = new UpdateConsentRequest { EmailMarketingConsent = consent };
        var response = await _httpClient.PutAsJsonAsync(
            "https://api.mercato.com/api/gdpr/consent/email-marketing", request);
        
        return await response.Content.ReadFromJsonAsync<ConsentResponse>();
    }

    public async Task<byte[]> ExportUserDataAsync(
        string accessToken, string format)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        var request = new DataExportRequest { Format = format };
        var response = await _httpClient.PostAsJsonAsync(
            "https://api.mercato.com/api/gdpr/export", request);
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<DeleteAccountResponse> DeleteAccountAsync(
        string accessToken, string email, string? reason = null)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        var request = new DeleteAccountRequest 
        { 
            ConfirmationEmail = email, 
            Reason = reason 
        };
        
        var response = await _httpClient.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri("https://api.mercato.com/api/gdpr/account"),
            Content = JsonContent.Create(request)
        });
        
        return await response.Content.ReadFromJsonAsync<DeleteAccountResponse>();
    }
}
```

---

## Related Documentation

- [GDPR Compliance Overview](GDPR_COMPLIANCE.md)
- [GDPR Data Minimization](GDPR_DATA_MINIMIZATION.md)
- [API Documentation](API_DOCUMENTATION.md)

---

**Last Updated:** 2025-11-23  
**Version:** 1.0
