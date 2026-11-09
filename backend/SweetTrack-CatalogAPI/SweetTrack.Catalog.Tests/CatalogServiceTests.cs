using Microsoft.EntityFrameworkCore;
using SweetTrack.Catalog.Application.DTOs.Common;
using SweetTrack.Catalog.Application.DTOs.Products;
using SweetTrack.Catalog.Application.Mappings;
using SweetTrack.Catalog.Application.Services;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Exceptions;

namespace SweetTrack.Catalog.Tests;

public class CatalogServiceTests
{
    private readonly CatalogDbContext _context;
    private readonly CatalogService _catalogService;

    public CatalogServiceTests()
    {
        MappingConfig.RegisterMappings();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CatalogDbContext(options);

        // Sembrar marcas y categorías de prueba
        var brand = new Brand { Id = 1, Name = "Arcor", IsActive = true };
        var category = new Category { Id = 1, Name = "Chocolates", Slug = "chocolates", IsActive = true };

        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.SaveChanges();

        _catalogService = new CatalogService(_context);
    }

    [Fact]
    public async Task CreateProductAsync_SuccessfullyCreatesProduct_AndRegistersInitialStockMovement()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Barcode = "7790123456789",
            Name = "Alfajor Bon o Bon Blanco",
            Description = "Alfajor relleno con pasta de maní y baño blanco.",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 500m,
            BoxPrice = 6000m,
            UnitsPerBox = 12,
            InitialStock = 120,
            SafetyStock = 20,
            IsActive = true
        };

        // Act
        var result = await _catalogService.CreateProductAsync(dto, "user-test");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("7790123456789", result.Barcode);
        Assert.Equal(120, result.StockUnits);
        Assert.Equal(500m, result.WholesalePrice);

        var movement = await _context.StockMovements.FirstOrDefaultAsync(m => m.ProductId == result.Id);
        Assert.NotNull(movement);
        Assert.Equal(120, movement.Quantity);
        Assert.Equal(120, movement.ResultingStock);
        Assert.Equal("user-test", movement.ResponsibleUserId);
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsDuplicateBarcodeException_WhenBarcodeAlreadyExists()
    {
        // Arrange
        var existing = new Product
        {
            Barcode = "7790000000001",
            Name = "Producto Existente",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 100m,
            BoxPrice = 1000m,
            UnitsPerBox = 10,
            StockUnits = 10,
            IsActive = true
        };
        _context.Products.Add(existing);
        await _context.SaveChangesAsync();

        var duplicateDto = new CreateProductDto
        {
            Barcode = "7790000000001",
            Name = "Otro Producto",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 200m,
            BoxPrice = 2000m
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DuplicateBarcodeException>(() =>
            _catalogService.CreateProductAsync(duplicateDto, "admin"));

        Assert.Equal("7790000000001", ex.Barcode);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersBySearchTermAndCategory_WithPagination()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Barcode = "A01", Name = "Chocolate Blanco Cofler", BrandId = 1, CategoryId = 1, WholesalePrice = 800m, StockUnits = 50, IsActive = true },
            new Product { Barcode = "A02", Name = "Chocolate Negro Cofler", BrandId = 1, CategoryId = 1, WholesalePrice = 850m, StockUnits = 30, IsActive = true },
            new Product { Barcode = "A03", Name = "Caramelo Masticable Arcor", BrandId = 1, CategoryId = 1, WholesalePrice = 150m, StockUnits = 100, IsActive = true }
        );
        await _context.SaveChangesAsync();

        var filter = new CatalogFilterParamsDto
        {
            SearchTerm = "Chocolate",
            CategoryId = 1,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _catalogService.GetProductsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, p => Assert.Contains("Chocolate", p.Name));
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Act
        var result = await _catalogService.GetProductByIdAsync(999999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProductAsync_PerformsSoftDelete()
    {
        // Arrange
        var product = new Product
        {
            Barcode = "DEL01",
            Name = "Producto a descontinuar",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 250m,
            BoxPrice = 2500m,
            StockUnits = 10,
            IsActive = true
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _catalogService.DeleteProductAsync(product.Id);

        // Assert
        Assert.True(deleted);

        var updated = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }
}
