namespace SweetTrack.Catalog.Application.DTOs.Common;

public class CatalogFilterParamsDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value <= 0 ? 1 : value);
    }

    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? OnlyInStock { get; set; }
    public bool? OnlyActive { get; set; } = true;
    public string? OrderBy { get; set; } // name_asc, name_desc, price_asc, price_desc
}

