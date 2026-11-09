namespace SweetTrack.Catalog.Domain.Entities;

/// <summary>
/// Producto del catálogo mayorista de golosinas y snacks.
/// Incluye token de concurrencia optimista (RowVersion) para operaciones atómicas de inventario.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal WholesalePrice { get; set; }
    public decimal BoxPrice { get; set; }
    public int UnitsPerBox { get; set; } = 1;
    public int StockUnits { get; set; }
    public int SafetyStock { get; set; } = 10;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    /// <summary>
    /// Indica si el producto se encuentra en o por debajo de su umbral de reposición.
    /// </summary>
    public bool IsLowStock => StockUnits <= SafetyStock;
}
