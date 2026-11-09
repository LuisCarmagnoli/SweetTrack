using Mapster;
using Microsoft.EntityFrameworkCore;
using SweetTrack.Catalog.Application.DTOs.Brands;
using SweetTrack.Catalog.Application.DTOs.Categories;
using SweetTrack.Catalog.Application.DTOs.Common;
using SweetTrack.Catalog.Application.DTOs.Products;
using SweetTrack.Catalog.Application.Interfaces;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Enums;
using SweetTrack.Catalog.Domain.Exceptions;

namespace SweetTrack.Catalog.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _context;

    public CatalogService(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<ProductResponseDto>> GetProductsAsync(CatalogFilterParamsDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsQueryable();

        if (filter.OnlyActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filter.OnlyActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term)
                                  || p.Barcode.ToLower().Contains(term)
                                  || (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.BrandId.HasValue)
        {
            query = query.Where(p => p.BrandId == filter.BrandId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.WholesalePrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.WholesalePrice <= filter.MaxPrice.Value);
        }

        if (filter.OnlyInStock == true)
        {
            query = query.Where(p => p.StockUnits > 0);
        }

        query = filter.OrderBy?.ToLower() switch
        {
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.WholesalePrice),
            "price_desc" => query.OrderByDescending(p => p.WholesalePrice),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectToType<ProductResponseDto>()
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ProductResponseDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product?.Adapt<ProductDetailDto>();
    }

    public async Task<ProductDetailDto?> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);

        return product?.Adapt<ProductDetailDto>();
    }

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        var barcodeExists = await _context.Products
            .AnyAsync(p => p.Barcode == dto.Barcode.Trim(), cancellationToken);

        if (barcodeExists)
        {
            throw new DuplicateBarcodeException(dto.Barcode.Trim());
        }

        var product = new Product
        {
            Barcode = dto.Barcode.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            BrandId = dto.BrandId,
            CategoryId = dto.CategoryId,
            WholesalePrice = dto.WholesalePrice,
            BoxPrice = dto.BoxPrice,
            UnitsPerBox = dto.UnitsPerBox <= 0 ? 1 : dto.UnitsPerBox,
            StockUnits = Math.Max(0, dto.InitialStock),
            SafetyStock = Math.Max(0, dto.SafetyStock),
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product, cancellationToken);

        if (product.StockUnits > 0)
        {
            var movement = new StockMovement
            {
                Product = product,
                MovementType = StockMovementType.IngresoManual,
                Quantity = product.StockUnits,
                ResultingStock = product.StockUnits,
                Reason = "Inventario inicial al crear producto",
                ResponsibleUserId = userId,
                TimestampUtc = DateTime.UtcNow
            };

            await _context.StockMovements.AddAsync(movement, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Recarga relaciones para el DTO
        await _context.Entry(product).Reference(p => p.Brand).LoadAsync(cancellationToken);
        await _context.Entry(product).Reference(p => p.Category).LoadAsync(cancellationToken);

        return product.Adapt<ProductDetailDto>();
    }

    public async Task<ProductDetailDto> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException(id);
        }

        var newBarcode = dto.Barcode.Trim();
        if (product.Barcode != newBarcode)
        {
            var barcodeTaken = await _context.Products
                .AnyAsync(p => p.Barcode == newBarcode && p.Id != id, cancellationToken);

            if (barcodeTaken)
            {
                throw new DuplicateBarcodeException(newBarcode);
            }

            product.Barcode = newBarcode;
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.BrandId = dto.BrandId;
        product.CategoryId = dto.CategoryId;
        product.WholesalePrice = dto.WholesalePrice;
        product.BoxPrice = dto.BoxPrice;
        product.UnitsPerBox = dto.UnitsPerBox <= 0 ? 1 : dto.UnitsPerBox;
        product.SafetyStock = Math.Max(0, dto.SafetyStock);
        product.ImageUrl = dto.ImageUrl;
        product.IsActive = dto.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        if (dto.RowVersion != null && dto.RowVersion.Length > 0)
        {
            _context.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductDetailDto>();
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product == null)
        {
            return false;
        }

        // Baja lógica
        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Brands
            .AsNoTracking()
            .Include(b => b.Products)
            .AsQueryable();

        if (onlyActive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query
            .OrderBy(b => b.Name)
            .ProjectToType<BrandDto>()
            .ToListAsync(cancellationToken);
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var brand = new Brand
        {
            Name = dto.Name.Trim(),
            LogoUrl = dto.LogoUrl,
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.Brands.AddAsync(brand, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return brand.Adapt<BrandDto>();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .AsQueryable();

        if (onlyActive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .ProjectToType<CategoryDto>()
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? dto.Name.Trim().ToLower().Replace(" ", "-")
            : dto.Slug.Trim().ToLower();

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Adapt<CategoryDto>();
    }
}
