using SweetTrack.Catalog.Domain.Enums;

namespace SweetTrack.Catalog.Domain.Entities;

/// <summary>
/// Registro inmutable de trazabilidad para cada variación de stock en el inventario.
/// </summary>
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int ResultingStock { get; set; }
    public string? Reason { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? ResponsibleUserId { get; set; }
}
