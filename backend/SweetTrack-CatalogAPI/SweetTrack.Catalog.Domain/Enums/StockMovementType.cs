namespace SweetTrack.Catalog.Domain.Enums;

/// <summary>
/// Tipos de movimiento de stock para auditoría y trazabilidad de inventario físico.
/// </summary>
public enum StockMovementType
{
    IngresoManual = 1,
    EgresoVenta = 2,
    AjusteInventario = 3,
    ImportacionMasivaExcel = 4,
    DevolucionPedido = 5
}

