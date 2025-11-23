using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Reports.Models;

/// <summary>
/// Represents the global commission configuration for the platform.
/// This allows administrators to set a system-wide default commission rate.
/// </summary>
public class GlobalCommissionConfig
{
    /// <summary>
    /// Unique identifier for the configuration record.
    /// In MVP, there will be only one record (singleton pattern).
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Global default commission rate (e.g., 0.15 for 15%).
    /// This is the base rate applied to all sellers unless overridden at store or category level.
    /// </summary>
    [Required]
    public decimal DefaultCommissionRate { get; set; } = 0.15m;

    /// <summary>
    /// Description or notes about the current commission structure.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if the configuration is active.
    /// For MVP, should always be true for the primary config.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User ID of the administrator who last modified this configuration.
    /// </summary>
    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Timestamp when this configuration was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this configuration was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
