namespace SweetTrack.Catalog.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta descontar una cantidad mayor al stock disponible en el inventario físico.
/// </summary>
public class InsufficientStockException : Exception
{
    public int ProductId { get; }
    public int RequestedUnits { get; }
    public int AvailableUnits { get; }

    public InsufficientStockException(int productId, int requestedUnits, int availableUnits)
        : base($"Stock insuficiente para el producto ID '{productId}'. Unidades requeridas: {requestedUnits}, unidades disponibles: {availableUnits}.")
    {
        ProductId = productId;
        RequestedUnits = requestedUnits;
        AvailableUnits = availableUnits;
    }
}

