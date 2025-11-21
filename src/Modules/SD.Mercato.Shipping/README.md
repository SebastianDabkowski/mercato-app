# SD.Mercato.Shipping Module

## Overview

The Shipping module is designed to support minimal shipment handling for the Mercato MVP while being extensible for future courier integrations and advanced features.

## Current MVP Implementation (Phase 1)

For the MVP, shipment data is stored directly in the `SubOrder` model in the History module:
- `CarrierName` - Carrier name entered by seller (optional)
- `TrackingNumber` - Tracking number entered by seller (optional)
- `ShippedAt` - Timestamp when marked as shipped

Sellers manually enter shipping details when marking orders as shipped. Buyers can view this information in their order history.

## Future Extensibility (Phase 2+)

The `Shipment` model in this module is designed to support future features:

### Planned Features

1. **Automated Label Generation**
   - Integration with carrier APIs (UPS, FedEx, USPS, etc.)
   - Generate shipping labels directly from the platform
   - Store label URLs for easy access

2. **Real-Time Tracking**
   - Poll carrier APIs for tracking updates
   - Store tracking events in `ShipmentTrackingEvent` table
   - Display real-time status to buyers

3. **Shipping Cost Calculation**
   - Use carrier APIs to calculate accurate shipping costs
   - Support negotiated rates
   - Weight and dimension-based calculations

4. **Advanced Features**
   - Package weight and dimensions
   - Service level selection (Ground, 2-Day, Overnight)
   - Return shipments
   - Multi-package orders
   - International shipping support

### Data Model Design

#### Shipment Table
- Links to SubOrder via `SubOrderId`
- Carrier information (name, service level, carrier shipment ID)
- Package details (weight, dimensions)
- Cost and dates (estimated/actual delivery)
- Label URL for generated labels
- Status tracking

#### ShipmentTrackingEvent Table
- Links to Shipment via `ShipmentId`
- Event type and description from carrier
- Location and timestamp
- Enables historical tracking view

## Migration Strategy

When implementing Phase 2:

1. Keep `SubOrder.CarrierName` and `SubOrder.TrackingNumber` for backward compatibility
2. Create new `Shipment` records for new shipments
3. Optionally migrate existing shipment data to the `Shipment` table
4. UI can check both sources and prioritize `Shipment` data when available

## Integration Points

### Current (MVP)
- **History Module**: SubOrder model stores basic carrier and tracking info
- **API**: `SellerOrdersController.UpdateSubOrderStatus` accepts carrier name and tracking number
- **UI**: Seller can enter carrier name and tracking number; buyer can view them

### Future (Phase 2+)
- **Shipping Module**: Manage Shipment entities and integrate with courier APIs
- **API**: New ShipmentController for label generation and tracking
- **UI**: Enhanced shipping workflows with label printing and real-time tracking
- **Background Jobs**: Poll carrier APIs for tracking updates

## Supported Carriers (Future)

Common carriers to integrate:
- UPS (UPS API)
- FedEx (FedEx Web Services)
- USPS (USPS Web Tools)
- DHL (DHL API)
- Regional carriers as needed

## Technical Notes

- Module follows DDD principles with clear domain boundaries
- Designed for minimal coupling with other modules
- Uses standard data annotations for validation
- Entity Framework Core ready for database migrations
- Supports async/await patterns for API calls

## Dependencies

Current:
- System.ComponentModel.DataAnnotations

Future:
- Carrier SDK packages (UPS API, FedEx SDK, etc.)
- Background job processing (Hangfire or similar)
- PDF generation for labels

## Documentation

For implementation details, see:
- `Models/Shipment.cs` - Core domain models
- `.github/copilot/business-domain.md` - Business requirements
- `.github/copilot-instructions.md` - Development guidelines
