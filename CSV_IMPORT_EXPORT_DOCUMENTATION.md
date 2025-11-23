# CSV Import/Export Documentation

## Overview

Mercato supports CSV-based import and export functionality for products and orders, enabling sellers to:
- Bulk import products from other systems
- Export products for backup or analysis
- Export orders for accounting and external integrations

All CSV operations are scoped to the authenticated seller's store and support future API integrations with platforms like Shopify, WooCommerce, and Baselinker.

---

## Product CSV Format

### Import/Export Fields

The product CSV format uses the following columns in order:

| Column Name | Type | Required | Description | Notes |
|------------|------|----------|-------------|-------|
| SKU | string | Yes | Stock Keeping Unit - unique identifier within your store | Max 100 characters |
| Title | string | Yes | Product title/name | Max 200 characters |
| Description | string | Yes | Product description | Max 5000 characters |
| Category | string | Yes | Category name (must match existing category) | Case-insensitive lookup |
| Price | decimal | Yes | Product price in USD | Must be > 0 |
| StockQuantity | integer | Yes | Available stock quantity | Cannot be negative |
| Weight | decimal | No | Product weight in grams | Optional, cannot be negative |
| Length | decimal | No | Product length in centimeters | Optional, cannot be negative |
| Width | decimal | No | Product width in centimeters | Optional, cannot be negative |
| Height | decimal | No | Product height in centimeters | Optional, cannot be negative |
| ImageUrls | string | No | Pipe-separated list of image URLs | Example: `url1\|url2\|url3` |
| Status | string | Yes | Product status | Must be: `Draft`, `Published`, or `Archived` |

### Import Behavior

- **Create**: If SKU doesn't exist in your store, a new product is created
- **Update**: If SKU already exists in your store, the product is updated with new values
- **Validation**: Each row is validated independently; errors are reported with row numbers

### Published Product Requirements

Published products must have:
- At least one image URL
- Valid category
- Price > 0
- Non-negative stock quantity

### Available Categories (MVP)

- Electronics
- Clothing
- Home & Garden
- Books
- Sports & Outdoors
- Toys & Games
- Health & Beauty
- Food & Beverages

### Example Product CSV

```csv
SKU,Title,Description,Category,Price,StockQuantity,Weight,Length,Width,Height,ImageUrls,Status
ELEC-001,Wireless Mouse,"Ergonomic wireless mouse with 2.4GHz connectivity",Electronics,29.99,150,85,12,8,4,https://example.com/mouse1.jpg|https://example.com/mouse2.jpg,Published
CLOTH-001,Cotton T-Shirt,"100% cotton crew neck t-shirt",Clothing,19.99,200,,,,,https://example.com/tshirt.jpg,Published
BOOK-001,Programming Guide,"Comprehensive programming guide for beginners",Books,49.99,50,400,24,16,2,,Draft
```

### API Endpoints

#### Download Template
```
GET /api/products/csv/template?includeSample=true
```
Downloads an empty CSV template with optional sample data.

**Authentication**: Required (Seller role)

**Response**: CSV file download

#### Import Products
```
POST /api/products/csv/import
Content-Type: multipart/form-data
```

**Request**: Form data with `file` parameter containing CSV file

**Authentication**: Required (Seller role)

**Response**:
```json
{
  "success": true,
  "successCount": 10,
  "failureCount": 2,
  "errors": [
    {
      "rowNumber": 5,
      "sku": "INVALID-001",
      "errorMessages": [
        "Price must be greater than 0",
        "Category 'Unknown' not found or inactive"
      ]
    }
  ],
  "message": "Imported 10 product(s) with 2 error(s)"
}
```

#### Export Products
```
GET /api/products/csv/export?status=Published&categoryId=<guid>
```

**Query Parameters**:
- `status` (optional): Filter by status (`Draft`, `Published`, `Archived`)
- `categoryId` (optional): Filter by category ID

**Authentication**: Required (Seller role)

**Response**: CSV file download

---

## Order CSV Format

### Export Fields

The order export CSV contains one row per product line item, with financial calculations:

| Column Name | Type | Description |
|------------|------|-------------|
| OrderNumber | string | Marketplace-level order number |
| SubOrderNumber | string | Seller-specific sub-order number |
| OrderDate | datetime | When the order was created (UTC) |
| Status | string | Sub-order status (Pending, Processing, Shipped, Delivered, Cancelled) |
| BuyerEmail | string | Buyer's email address |
| BuyerPhone | string | Buyer's phone number |
| RecipientName | string | Delivery recipient name |
| DeliveryAddress | string | Full formatted delivery address |
| ProductSKU | string | Product SKU |
| ProductTitle | string | Product title |
| Quantity | integer | Quantity ordered |
| UnitPrice | decimal | Price per unit |
| LineSubtotal | decimal | Line total (Quantity × UnitPrice) |
| ShippingCost | decimal | Shipping cost (shown on first line only) |
| SubOrderTotal | decimal | Total for sub-order (shown on first line only) |
| PlatformCommission | decimal | 15% commission on product total (shown on first line only) |
| ProcessingFee | decimal | 2.9% + $0.30 gateway fee (shown on first line only) |
| NetAmount | decimal | Amount seller receives (shown on first line only) |
| ShippingMethod | string | Selected shipping method |
| TrackingNumber | string | Tracking number (if shipped) |
| ShippedDate | datetime | Date when shipped (if applicable) |
| DeliveredDate | datetime | Date when delivered (if applicable) |

### Commission and Fee Calculations

**Platform Commission**: 15% of product total (not including shipping)

**Processing Fee**: 2.9% + $0.30 on total amount (products + shipping)

**Net Amount**: `Total - Commission - Processing Fee`

#### Example Calculation

For a sub-order with:
- Products Total: $100.00
- Shipping Cost: $10.00
- **Total Amount**: $110.00

Calculations:
- **Platform Commission**: $100.00 × 15% = $15.00
- **Processing Fee**: ($110.00 × 2.9%) + $0.30 = $3.49
- **Net Amount**: $110.00 - $15.00 - $3.49 = **$91.51**

### Multi-Item Orders

When a sub-order contains multiple products:
- Each product gets its own row
- Financial totals (ShippingCost, SubOrderTotal, PlatformCommission, ProcessingFee, NetAmount) appear on the first row only
- Subsequent rows show 0 for these fields to avoid double-counting

### Example Order CSV

```csv
OrderNumber,SubOrderNumber,OrderDate,Status,BuyerEmail,BuyerPhone,RecipientName,DeliveryAddress,ProductSKU,ProductTitle,Quantity,UnitPrice,LineSubtotal,ShippingCost,SubOrderTotal,PlatformCommission,ProcessingFee,NetAmount,ShippingMethod,TrackingNumber,ShippedDate,DeliveredDate
MKT-2024-001,SUB-2024-001,2024-11-23T10:30:00Z,Shipped,buyer@example.com,555-0123,John Doe,"123 Main St, New York, NY 10001, United States",ELEC-001,Wireless Mouse,2,29.99,59.98,10.00,69.98,9.00,2.33,58.65,Standard Shipping,1Z999AA10123456784,2024-11-24T14:20:00Z,
MKT-2024-001,SUB-2024-001,2024-11-23T10:30:00Z,Shipped,buyer@example.com,555-0123,John Doe,"123 Main St, New York, NY 10001, United States",ELEC-002,USB Cable,3,5.99,17.97,0,0,0,0,0,Standard Shipping,1Z999AA10123456784,2024-11-24T14:20:00Z,
```

### API Endpoint

#### Export Orders
```
GET /api/seller/orders/csv/export?fromDate=2024-01-01&toDate=2024-12-31&status=Delivered
```

**Query Parameters**:
- `fromDate` (optional): Start date for order filtering (inclusive, format: `YYYY-MM-DD`)
- `toDate` (optional): End date for order filtering (inclusive, format: `YYYY-MM-DD`)
- `status` (optional): Filter by status (`Pending`, `Processing`, `Shipped`, `Delivered`, `Completed`, `Cancelled`)

**Authentication**: Required (Seller role)

**Response**: 
- If orders found: CSV file download
- If no orders: JSON response with message and recordCount = 0

---

## Best Practices

### For Product Import

1. **Always download the template first** to ensure correct column order
2. **Test with small batches** before importing large datasets
3. **Use unique SKUs** within your store to avoid conflicts
4. **Validate categories** match the available category names (case-insensitive)
5. **Include images for published products** to meet platform requirements
6. **Review error messages** carefully - they include row numbers for easy correction

### For Order Export

1. **Export regularly** for accounting and backup purposes
2. **Use date ranges** to manage file sizes for large order volumes
3. **Filter by status** to export specific order states (e.g., only delivered orders for accounting)
4. **Note the multi-row format** - sum LineSubtotal, not SubOrderTotal, when analyzing in Excel
5. **Commission calculations are accurate** and match payout reports

### Data Integrity

- **SKU consistency**: Maintain consistent SKU format across all platforms
- **Category mapping**: Create a mapping table if migrating from systems with different categories
- **Image hosting**: Ensure image URLs are publicly accessible and permanent
- **Backup exports**: Export products monthly for disaster recovery

---

## Future API Integration

The CSV format is designed to support future API-based integrations with:

### Supported Platforms (Planned)

- **Shopify**: Map SKU to Shopify variant SKU
- **WooCommerce**: Map categories to WooCommerce categories
- **Baselinker**: Use SKU as primary identifier
- **Custom ERPs**: Standard CSV format works with most business systems

### Integration Considerations

1. **SKU as primary key**: All integrations will use SKU as the unique identifier
2. **Category mapping**: Future integrations will support category ID mapping
3. **Image sync**: API integrations can automatically sync image URLs
4. **Stock sync**: Real-time stock updates will be supported via API
5. **Order sync**: Order data can be pushed to accounting systems automatically

### API Migration Path

Current CSV operations are implemented as a foundation for:
- REST API endpoints for programmatic access
- Webhook support for real-time sync
- OAuth authentication for third-party platforms
- Rate limiting and bulk operation queuing

---

## Troubleshooting

### Common Import Errors

| Error Message | Cause | Solution |
|--------------|-------|----------|
| "SKU is required" | Empty SKU column | Ensure all rows have a SKU value |
| "Category 'X' not found or inactive" | Invalid category name | Use one of the available category names |
| "Price must be greater than 0" | Invalid or zero price | Set a positive price value |
| "Published products must have at least one image" | Missing images for published product | Add image URLs or change status to Draft |
| "Invalid status. Must be one of: ..." | Invalid status value | Use Draft, Published, or Archived |

### Export Issues

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| No data exported | No orders match filters | Adjust date range or status filter |
| Missing orders | Orders from other sellers | You can only export your own store's orders |
| Wrong calculations | Reviewing wrong columns | Use NetAmount column for what you'll receive |

---

## Technical Notes

### Character Encoding
All CSV files use UTF-8 encoding to support international characters.

### Date Format
Dates in exports use ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ) in UTC timezone.

### Decimal Precision
All monetary values are stored with 2 decimal places.

### CSV Parsing
- Headers are required and case-sensitive
- Commas are the field delimiter
- Fields containing commas are quoted
- Pipe character (`|`) separates multiple image URLs

### Security
- All endpoints require authentication
- Users can only import/export data for their own store
- File uploads are size-limited (check API configuration)
- Malicious content in CSV files is rejected

---

## Support

For assistance with CSV operations:
1. Review this documentation
2. Check error messages for specific row numbers
3. Verify your CSV matches the template format
4. Contact platform support with the specific error message and row number

---

**Last Updated**: November 2024  
**Version**: 1.0  
**Supported in**: MVP Release
