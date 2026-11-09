using SweetTrack.Catalog.Application.DTOs.Stock;

namespace SweetTrack.Catalog.Application.Interfaces;

public interface IStockService
{
    Task<StockMovementDto> AdjustStockAsync(int productId, StockAdjustmentDto dto, string? userId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovementDto>> GetProductStockHistoryAsync(int productId, int limit = 50, CancellationToken cancellationToken = default);
}
