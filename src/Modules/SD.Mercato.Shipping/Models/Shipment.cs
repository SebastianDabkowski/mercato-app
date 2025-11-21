using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Shipping.Models;

/// <summary>
/// Represents a shipment for tracking and future courier integration.
/// Designed to support future extensions like label generation and real-time tracking.
/// </summary>
public class Shipment
{
    /// <summary>
    /// Unique identifier for the shipment.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// SubOrder ID this shipment belongs to.
    /// </summary>
    [Required]
    public Guid SubOrderId { get; set; }

    /// <summary>
    /// Carrier name (e.g., "UPS", "FedEx", "USPS").
    /// </summary>
    [MaxLength(100)]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Tracking number provided by the carrier.
    /// </summary>
    [MaxLength(200)]
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Carrier service level (e.g., "Ground", "2-Day Air", "Overnight").
    /// Reserved for future use with courier integrations.
    /// </summary>
    [MaxLength(100)]
    public string? ServiceLevel { get; set; }

    /// <summary>
    /// Shipping label URL or file path.
    /// Reserved for future automated label generation.
    /// </summary>
    [MaxLength(500)]
    public string? LabelUrl { get; set; }

    /// <summary>
    /// Estimated delivery date provided by carrier.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>
    /// Actual delivery date (when package was delivered).
    /// </summary>
    public DateTime? ActualDeliveryDate { get; set; }

    /// <summary>
    /// Shipment status (e.g., "Created", "InTransit", "OutForDelivery", "Delivered", "Exception").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ShipmentStatus.Created;

    /// <summary>
    /// External carrier shipment ID.
    /// Reserved for future API integrations with courier services.
    /// </summary>
    [MaxLength(200)]
    public string? CarrierShipmentId { get; set; }

    /// <summary>
    /// Shipping cost (may differ from SubOrder shipping cost if negotiated rates apply).
    /// </summary>
    public decimal? ShippingCost { get; set; }

    /// <summary>
    /// Package weight in pounds.
    /// Reserved for future automated shipping calculations.
    /// </summary>
    public decimal? WeightPounds { get; set; }

    /// <summary>
    /// Package dimensions (Length x Width x Height) in inches.
    /// Reserved for future automated shipping calculations.
    /// </summary>
    [MaxLength(50)]
    public string? Dimensions { get; set; }

    /// <summary>
    /// Timestamp when the shipment was created.
    /// Note: When integrating with EF Core, set this in service layer or DbContext instead of using default value.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the shipment was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when the shipment was marked as shipped.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Additional notes or special instructions.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Collection of tracking events (for future real-time tracking integration).
    /// </summary>
    public List<ShipmentTrackingEvent> TrackingEvents { get; set; } = new();
}

/// <summary>
/// Shipment status constants.
/// </summary>
public static class ShipmentStatus
{
    public const string Created = "Created";
    public const string LabelGenerated = "LabelGenerated";
    public const string PickedUp = "PickedUp";
    public const string InTransit = "InTransit";
    public const string OutForDelivery = "OutForDelivery";
    public const string Delivered = "Delivered";
    public const string Exception = "Exception";
    public const string Returned = "Returned";
}

/// <summary>
/// Represents a tracking event for a shipment (e.g., package scanned at facility).
/// Reserved for future real-time tracking integrations.
/// </summary>
public class ShipmentTrackingEvent
{
    /// <summary>
    /// Unique identifier for the tracking event.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Shipment ID this event belongs to.
    /// </summary>
    [Required]
    public Guid ShipmentId { get; set; }

    /// <summary>
    /// Navigation property to parent Shipment.
    /// </summary>
    public Shipment? Shipment { get; set; }

    /// <summary>
    /// Event type (e.g., "Picked Up", "In Transit", "Out for Delivery", "Delivered").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Event description from carrier.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Location where event occurred (e.g., "Memphis, TN", "Distribution Center").
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// Timestamp of the tracking event.
    /// </summary>
    [Required]
    public DateTime EventTimestamp { get; set; }

    /// <summary>
    /// Timestamp when this event was recorded in our system.
    /// Note: When integrating with EF Core, set this in service layer or DbContext instead of using default value.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
