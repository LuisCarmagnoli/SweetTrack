using Mapster;
using SweetTrack.Catalog.Application.DTOs.Brands;
using SweetTrack.Catalog.Application.DTOs.Categories;
using SweetTrack.Catalog.Application.DTOs.Products;
using SweetTrack.Catalog.Application.DTOs.Stock;
using SweetTrack.Catalog.Domain.Entities;

namespace SweetTrack.Catalog.Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Product, ProductResponseDto>.NewConfig()
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : string.Empty)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.IsLowStock, src => src.IsLowStock);

        TypeAdapterConfig<Product, ProductDetailDto>.NewConfig()
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : string.Empty)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.IsLowStock, src => src.IsLowStock);

        TypeAdapterConfig<Category, CategoryDto>.NewConfig()
            .Map(dest => dest.ProductsCount, src => src.Products.Count);

        TypeAdapterConfig<Brand, BrandDto>.NewConfig()
            .Map(dest => dest.ProductsCount, src => src.Products.Count);

        TypeAdapterConfig<StockMovement, StockMovementDto>.NewConfig()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty);
    }
}

