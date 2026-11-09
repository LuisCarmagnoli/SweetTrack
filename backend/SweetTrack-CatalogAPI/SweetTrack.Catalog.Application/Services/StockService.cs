using Mapster;
using Microsoft.EntityFrameworkCore;
using SweetTrack.Catalog.Application.DTOs.Stock;
using SweetTrack.Catalog.Application.Interfaces;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Exceptions;

namespace SweetTrack.Catalog.Application.Services;

public class StockService : IStockService
{
    private readonly CatalogDbContext _context;

    public StockService(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovementDto> AdjustStockAsync(int productId, StockAdjustmentDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException(productId);
        }

        var resultingStock = product.StockUnits + dto.Quantity;
        if (resultingStock < 0)
        {
            throw new InsufficientStockException(productId, Math.Abs(dto.Quantity), product.StockUnits);
        }

        product.StockUnits = resultingStock;
        product.UpdatedAtUtc = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = productId,
            Product = product,
            MovementType = dto.MovementType,
            Quantity = dto.Quantity,
            ResultingStock = resultingStock,
            Reason = dto.Reason ?? $"Ajuste de stock: {dto.Quantity} unidades",
            ResponsibleUserId = userId,
            TimestampUtc = DateTime.UtcNow
        };

        await _context.StockMovements.AddAsync(movement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return movement.Adapt<StockMovementDto>();
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetProductStockHistoryAsync(int productId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.TimestampUtc)
            .Take(Math.Min(100, Math.Max(1, limit)))
            .ProjectToType<StockMovementDto>()
            .ToListAsync(cancellationToken);

        return movements;
    }
}
