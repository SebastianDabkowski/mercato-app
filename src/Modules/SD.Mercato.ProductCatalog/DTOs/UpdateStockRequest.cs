using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.DTOs;

/// <summary>
/// Request DTO for updating product stock via partner API.
/// </summary>
public class UpdateStockRequest
{
    /// <summary>
    /// New stock quantity.
    /// </summary>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }
}
