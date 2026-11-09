using Microsoft.EntityFrameworkCore;
using SweetTrack.Catalog.Application.DTOs.Stock;
using SweetTrack.Catalog.Application.Mappings;
using SweetTrack.Catalog.Application.Services;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Enums;
using SweetTrack.Catalog.Domain.Exceptions;

namespace SweetTrack.Catalog.Tests;

public class StockServiceTests
{
    private readonly CatalogDbContext _context;
    private readonly StockService _stockService;

    public StockServiceTests()
    {
        MappingConfig.RegisterMappings();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CatalogDbContext(options);

        var brand = new Brand { Id = 1, Name = "Felfort", IsActive = true };
        var category = new Category { Id = 1, Name = "Chocolates", Slug = "chocolates", IsActive = true };

        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.SaveChanges();

        _stockService = new StockService(_context);
    }

    [Fact]
    public async Task AdjustStockAsync_IncreasesStock_OnPositiveQuantity()
    {
        // Arrange
        var product = new Product
        {
            Barcode = "STK01",
            Name = "Marroc Clásico",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 450m,
            BoxPrice = 27000m,
            StockUnits = 20,
            IsActive = true
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var adjustment = new StockAdjustmentDto
        {
            Quantity = 30,
            MovementType = StockMovementType.IngresoManual,
            Reason = "Ingreso de bultos por compra proveedor"
        };

        // Act
        var result = await _stockService.AdjustStockAsync(product.Id, adjustment, "almacen-admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.ResultingStock);
        Assert.Equal(30, result.Quantity);
        Assert.Equal(StockMovementType.IngresoManual, result.MovementType);

        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(50, updatedProduct.StockUnits);
    }

    [Fact]
    public async Task AdjustStockAsync_DecreasesStock_OnNegativeQuantity()
    {
        // Arrange
        var product = new Product
        {
            Barcode = "STK02",
            Name = "Jack con Muñeco",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 1100m,
            BoxPrice = 13200m,
            StockUnits = 40,
            IsActive = true
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var adjustment = new StockAdjustmentDto
        {
            Quantity = -15,
            MovementType = StockMovementType.EgresoVenta,
            Reason = "Despacho pedido mayorista"
        };

        // Act
        var result = await _stockService.AdjustStockAsync(product.Id, adjustment, "despacho-user");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.ResultingStock);
        Assert.Equal(-15, result.Quantity);

        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(25, updatedProduct.StockUnits);
    }

    [Fact]
    public async Task AdjustStockAsync_ThrowsInsufficientStockException_WhenQuantityExceedsAvailable()
    {
        // Arrange
        var product = new Product
        {
            Barcode = "STK03",
            Name = "Dos Corazones",
            BrandId = 1,
            CategoryId = 1,
            WholesalePrice = 600m,
            BoxPrice = 14400m,
            StockUnits = 5,
            IsActive = true
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var adjustment = new StockAdjustmentDto
        {
            Quantity = -10, // Intenta descontar 10 cuando solo hay 5
            MovementType = StockMovementType.EgresoVenta
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
            _stockService.AdjustStockAsync(product.Id, adjustment, "user"));

        Assert.Equal(product.Id, ex.ProductId);
        Assert.Equal(10, ex.RequestedUnits);
        Assert.Equal(5, ex.AvailableUnits);
    }

    [Fact]
    public async Task AdjustStockAsync_ThrowsProductNotFoundException_WhenProductDoesNotExist()
    {
        var adjustment = new StockAdjustmentDto
        {
            Quantity = 10,
            MovementType = StockMovementType.IngresoManual
        };

        await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            _stockService.AdjustStockAsync(999999, adjustment, "user"));
    }
}
