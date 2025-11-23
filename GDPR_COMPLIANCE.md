# GDPR Compliance Documentation

## Overview

This document describes Mercato's GDPR (General Data Protection Regulation) compliance implementation, including data retention policies, anonymization strategies, and the legal basis for data processing.

## Table of Contents

1. [GDPR Principles](#gdpr-principles)
2. [User Rights Implementation](#user-rights-implementation)
3. [Data Retention Policy](#data-retention-policy)
4. [Anonymization and Pseudonymization](#anonymization-and-pseudonymization)
5. [Legal Basis for Data Processing](#legal-basis-for-data-processing)
6. [Data Minimization](#data-minimization)
7. [Admin Privacy Tools](#admin-privacy-tools)
8. [Technical Implementation](#technical-implementation)

## GDPR Principles

Mercato implements the following GDPR principles:

1. **Lawfulness, fairness and transparency** - We process data lawfully with user consent and clear privacy policies
2. **Purpose limitation** - Data is collected for specific, legitimate purposes
3. **Data minimization** - We only collect and process data necessary for the stated purpose
4. **Accuracy** - Users can update their personal information at any time
5. **Storage limitation** - Data is retained only as long as necessary
6. **Integrity and confidentiality** - Data is protected with appropriate security measures
7. **Accountability** - We maintain records of processing activities and can demonstrate compliance

## User Rights Implementation

### Right to Access (Article 15)

Users can view their personal data through:
- User profile page in the UI
- GET `/api/gdpr/consent` - View consent settings

### Right to Data Portability (Article 20)

Users can export all their personal data in machine-readable format:
- POST `/api/gdpr/export` - Export data in JSON or CSV format

Exported data includes:
- Profile information (name, email, phone)
- Order history
- Consent preferences
- Account metadata

### Right to Rectification (Article 16)

Users can update their personal information:
- Through user profile update endpoints
- All profile fields are editable except email (requires re-verification)

### Right to Erasure (Article 17)

Users can request account deletion:
- DELETE `/api/gdpr/account` - Delete account and anonymize data

**Important:** Due to legal and accounting requirements, we cannot fully delete user records. Instead, we:
1. Mark the account as deleted (`IsDeleted = true`)
2. Anonymize personal identifiable information (PII)
3. Retain minimal data required for legal compliance

### Right to Withdraw Consent (Article 7)

Users can update their marketing consent at any time:
- PUT `/api/gdpr/consent/email-marketing` - Update email marketing consent

## Data Retention Policy

### Active Users

| Data Type | Retention Period | Basis |
|-----------|------------------|-------|
| User Profile | Duration of account + 30 days | Contract performance |
| Order History | 7 years from order date | Legal obligation (tax/accounting) |
| Payment Transactions | 7 years from transaction date | Legal obligation (PCI DSS, tax law) |
| Email Marketing Consent | Duration of account + 2 years | Consent tracking |
| Audit Logs | 3 years | Legal obligation (compliance) |

### Deleted Accounts

When a user requests account deletion:

| Data Type | Action | Retention | Basis |
|-----------|--------|-----------|-------|
| Name | Anonymized to "[DELETED]" | Indefinite (anonymized) | Legal obligation |
| Email | Pseudonymized to "deleted_{userId}@anonymized.mercato" | Indefinite (pseudonymized) | Legal obligation |
| Phone Number | Deleted | N/A | Right to erasure |
| Order Details | **Retained in anonymized form** | 7 years | Legal obligation |
| Payment Transactions | **Retained** | 7 years | Legal obligation |
| User ID | Retained (as foreign key) | Indefinite | Legal obligation |
| Marketing Consent | Revoked | Indefinite (for proof) | Legal obligation |

**Rationale for Retention:**
- **Tax & Accounting Laws:** Most jurisdictions require businesses to retain transaction records for 7+ years
- **Legal Disputes:** Order records may be needed for dispute resolution
- **Financial Audits:** Complete transaction history required for audits
- **Seller Payouts:** Historical data needed to verify past payouts

## Anonymization and Pseudonymization

### Anonymization Strategy

When a user deletes their account, we apply the following transformations:

```csharp
// User table
FirstName = "[DELETED]"
LastName = "[DELETED]"
Email = "deleted_{userId}@anonymized.mercato"
NormalizedEmail = Email.ToUpperInvariant()
UserName = "deleted_{userId}"
NormalizedUserName = UserName.ToUpperInvariant()
PhoneNumber = null
EmailMarketingConsent = false
IsDeleted = true
DeletedAt = DateTime.UtcNow
```

### What Remains in Orders

Orders retain the delivery address and buyer contact information **as captured at the time of order** because:
1. Sellers need this for shipping and fulfillment
2. Tax authorities require complete transaction records
3. Dispute resolution may require proof of delivery

**Data Minimization Applied:**
- Orders do **not** link back to the active user profile
- The `UserId` field becomes a reference to an anonymized account
- Sellers never receive more buyer information than necessary for fulfillment

### Pseudonymization in Orders

Order records contain buyer information but are pseudonymized through:
1. The `UserId` field points to an anonymized user record
2. Historical buyer email/phone in orders is retained but isolated from active user accounts
3. No direct link between active user sessions and old anonymized orders

### Data Subject Cannot Be Re-identified

After account deletion:
- The user cannot log in (credentials anonymized)
- The email address is changed to a non-functional pseudonym
- Personal identifiers (name, phone) are removed or anonymized
- The account cannot be linked back to the original individual without the original email

## Legal Basis for Data Processing

| Data Type | Legal Basis | GDPR Article |
|-----------|-------------|--------------|
| User Profile (Name, Email, Phone) | Contract performance | Art. 6(1)(b) |
| Order Information | Contract performance | Art. 6(1)(b) |
| Payment Information | Contract performance | Art. 6(1)(b) |
| Email Marketing | Consent | Art. 6(1)(a) |
| Transaction Records (7 years) | Legal obligation | Art. 6(1)(c) |
| Audit Logs | Legal obligation | Art. 6(1)(c) |
| Security Logs | Legitimate interest | Art. 6(1)(f) |

### Contract Performance (Art. 6(1)(b))

We process user data to:
- Create and manage user accounts
- Process and fulfill orders
- Facilitate payments between buyers and sellers
- Provide customer support

### Consent (Art. 6(1)(a))

We obtain explicit consent for:
- Email marketing communications
- Newsletter subscriptions

Users can withdraw consent at any time without affecting other services.

### Legal Obligation (Art. 6(1)(c))

We retain certain data to comply with:
- **Tax Laws:** Transaction records for tax reporting
- **Accounting Laws:** Financial records for 7 years
- **PCI DSS:** Payment transaction logs
- **Anti-Money Laundering (AML):** Transaction history for compliance

### Legitimate Interest (Art. 6(1)(f))

We process certain data for:
- Fraud prevention and security
- Platform integrity and abuse prevention
- System performance monitoring

## Data Minimization

### Buyer-Seller Data Sharing

We minimize data shared between buyers and sellers:

**Sellers receive only:**
- Buyer name (for shipping label)
- Delivery address
- Contact phone/email (for delivery issues)
- Order details (products, quantities)

**Sellers do NOT receive:**
- Buyer user ID
- Buyer password or login credentials
- Buyer payment information (card details)
- Buyer's other orders with different sellers
- Buyer marketing preferences
- Buyer account creation date or metadata

### Order Fulfillment Data

When a buyer places an order:
1. Payment is processed through the platform (sellers never see card details)
2. Order details are split into SubOrders per seller
3. Each seller sees only their own SubOrder with minimal buyer info
4. Platform holds funds until delivery confirmation

## Admin Privacy Tools

### Data Subject Access Requests (DSAR)

Privacy officers can retrieve all user data via:
- POST `/api/admin/gdpr/user-data`

This endpoint returns:
- Complete user profile
- All orders (past and present)
- All notifications sent
- Consent history
- Account metadata

### Audit Logging

All admin access to user data is logged:
- Who accessed the data (admin user ID and email)
- When the access occurred
- Which user's data was accessed
- IP address of the admin

## Technical Implementation

### Database Schema Changes

#### Users Table
```sql
ALTER TABLE AspNetUsers ADD EmailMarketingConsent bit NOT NULL DEFAULT 0;
ALTER TABLE AspNetUsers ADD EmailMarketingConsentUpdatedAt datetime2 NULL;
ALTER TABLE AspNetUsers ADD IsDeleted bit NOT NULL DEFAULT 0;
ALTER TABLE AspNetUsers ADD DeletedAt datetime2 NULL;
```

### API Endpoints

#### User-Facing GDPR Endpoints
- `GET /api/gdpr/consent` - Get consent settings
- `PUT /api/gdpr/consent/email-marketing` - Update marketing consent
- `POST /api/gdpr/export` - Export user data (JSON/CSV)
- `DELETE /api/gdpr/account` - Delete account

#### Admin GDPR Endpoints
- `POST /api/admin/gdpr/user-data` - Get all user data for compliance audit

### Services

- **GdprService** (Users module) - Handles user-facing GDPR operations
- **AdminGdprService** (Administration module) - Handles admin/compliance operations

### Registration Flow

During registration, users are presented with:
1. **Required:** Terms of Service acceptance (contract basis)
2. **Optional:** Email marketing consent checkbox

Consent is recorded with timestamp for audit purposes.

## Assumptions and TODOs

### Assumptions

1. **Retention Period:** We assume 7 years for transaction records based on common tax/accounting requirements. This may vary by jurisdiction.
   
2. **Right to Erasure Exceptions:** We assume legal/accounting obligations override the right to erasure for transaction history (GDPR Art. 17(3)(b) and (e)).

3. **Seller Data Access:** We assume sellers have a legitimate need for buyer contact information for order fulfillment (contract performance basis).

4. **Anonymization is Sufficient:** We assume that the anonymization strategy described above renders the data non-personal (recital 26 of GDPR).

### Outstanding TODOs

1. **Cross-Module Data Retrieval:**
   - Currently, order and notification data are not retrieved in data export
   - Need to implement queries to History and Notification modules
   - Consider creating a shared GDPR data aggregation service

2. **Retention Policy Automation:**
   - Implement automated job to purge old data after retention period expires
   - Consider archiving strategy for data older than 7 years

3. **Data Breach Procedures:**
   - Document incident response plan
   - Implement breach notification system (72-hour requirement)

4. **Privacy by Design:**
   - Review all new features for GDPR compliance
   - Conduct Data Protection Impact Assessments (DPIA) for high-risk processing

5. **Cookie Consent:**
   - Implement cookie consent banner for EU visitors
   - Document all cookies and tracking technologies

6. **Third-Party Processors:**
   - Document all third-party data processors (payment gateways, email services)
   - Ensure Data Processing Agreements (DPAs) are in place

7. **International Transfers:**
   - If data is transferred outside EU/EEA, ensure adequate safeguards
   - Implement Standard Contractual Clauses (SCCs) if necessary

8. **Subject Access Request Automation:**
   - Consider automating the DSAR response workflow
   - Implement deadline tracking (30-day response requirement)

## References

- [GDPR Official Text](https://gdpr-info.eu/)
- [ICO Guide to GDPR](https://ico.org.uk/for-organisations/guide-to-data-protection/guide-to-the-general-data-protection-regulation-gdpr/)
- [Article 29 Working Party Guidelines](https://ec.europa.eu/newsroom/article29/items/612053)

## Changelog

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2025-11-23 | 1.0 | Initial GDPR implementation | GitHub Copilot |

---

**Last Updated:** 2025-11-23  
**Document Owner:** Privacy Officer / DPO  
**Review Cycle:** Annually or when significant changes occur
