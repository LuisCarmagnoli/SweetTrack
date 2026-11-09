using SweetTrack.Catalog.Application.DTOs.Brands;
using SweetTrack.Catalog.Application.DTOs.Categories;
using SweetTrack.Catalog.Application.DTOs.Common;
using SweetTrack.Catalog.Application.DTOs.Products;

namespace SweetTrack.Catalog.Application.Interfaces;

public interface ICatalogService
{
    Task<PagedResultDto<ProductResponseDto>> GetProductsAsync(CatalogFilterParamsDto filter, CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto, string? userId = null, CancellationToken cancellationToken = default);
    Task<ProductDetailDto> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BrandDto>> GetBrandsAsync(bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
}

