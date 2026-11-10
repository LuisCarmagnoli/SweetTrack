using SweetTrack.Catalog.Domain.Enums;

namespace SweetTrack.Catalog.Application.DTOs.Stock;

public class StockAdjustmentDto
{
    public int Quantity { get; set; }
    public StockMovementType MovementType { get; set; } = StockMovementType.AjusteInventario;
    public string? Reason { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public string MovementTypeName => MovementType.ToString();
    public int Quantity { get; set; }
    public int ResultingStock { get; set; }
    public string? Reason { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? ResponsibleUserId { get; set; }
}

