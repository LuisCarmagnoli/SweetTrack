namespace SweetTrack.Catalog.Application.DTOs.Products;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal WholesalePrice { get; set; }
    public decimal BoxPrice { get; set; }
    public int UnitsPerBox { get; set; }
    public int StockUnits { get; set; }
    public int SafetyStock { get; set; }
    public bool IsLowStock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDetailDto : ProductResponseDto
{
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class CreateProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal BoxPrice { get; set; }
    public int UnitsPerBox { get; set; } = 1;
    public int InitialStock { get; set; } = 0;
    public int SafetyStock { get; set; } = 10;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal BoxPrice { get; set; }
    public int UnitsPerBox { get; set; } = 1;
    public int SafetyStock { get; set; } = 10;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }
}

public class UpdatePriceDto
{
    public decimal WholesalePrice { get; set; }
    public decimal BoxPrice { get; set; }
}
