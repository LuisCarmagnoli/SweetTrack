namespace SweetTrack.Ordering.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    
    public string CodigoBarrasSnapshot { get; set; } = string.Empty;
    public string NombreProductoSnapshot { get; set; } = string.Empty;
    
    public int CantidadUnidades { get; set; }
    public int CantidadBultos { get; set; }
    
    public decimal PrecioUnitarioSnapshot { get; set; }
    public decimal SubtotalLinea { get; set; }

    // Propiedad de navegación
    public Order Order { get; set; } = null!;

    public OrderItem() { }

    public OrderItem(
        Guid productId,
        string barcode,
        string productName,
        int units,
        int boxes,
        decimal unitPrice)
    {
        ProductId = productId;
        CodigoBarrasSnapshot = barcode;
        NombreProductoSnapshot = productName;
        CantidadUnidades = units;
        CantidadBultos = boxes;
        PrecioUnitarioSnapshot = unitPrice;
        SubtotalLinea = units * unitPrice;
    }
}
