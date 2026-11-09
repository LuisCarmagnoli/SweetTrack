namespace SweetTrack.Catalog.Domain.Entities;

/// <summary>
/// Marca mayorista o fabricante líder (ej. Arcor, Mondelez, Felfort, Terrabusi).
/// </summary>
public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

