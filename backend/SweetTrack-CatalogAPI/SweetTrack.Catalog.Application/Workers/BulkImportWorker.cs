using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SweetTrack.Catalog.Application.DTOs.BulkImport;
using SweetTrack.Catalog.Application.Interfaces;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Enums;

namespace SweetTrack.Catalog.Application.Workers;

public class BulkImportWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBulkProductImportChannel _channel;
    private readonly ILogger<BulkImportWorker> _logger;

    public BulkImportWorker(
        IServiceScopeFactory scopeFactory,
        IBulkProductImportChannel channel,
        ILogger<BulkImportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _channel = channel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando BulkImportWorker de SweetTrack Catalog...");

        // Configuración de licencia no comercial de EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        await foreach (var task in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessImportTaskAsync(task, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar la tarea de importación masiva {TaskId}", task.TaskId);

                _channel.UpdateTaskStatus(task.TaskId, status =>
                {
                    status.Status = BulkImportTaskStatus.Fallido;
                    status.FinishedAtUtc = DateTime.UtcNow;
                    status.ErrorMessages.Add($"Error crítico inesperado: {ex.Message}");
                });
            }
        }

        _logger.LogInformation("Deteniendo BulkImportWorker.");
    }

    private async Task ProcessImportTaskAsync(BulkImportTask task, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Procesando tarea de importación {TaskId} para el archivo {FileName}", task.TaskId, task.FileName);

        _channel.UpdateTaskStatus(task.TaskId, status =>
        {
            status.Status = BulkImportTaskStatus.Procesando;
            status.StartedAtUtc = DateTime.UtcNow;
        });

        using var memoryStream = new MemoryStream(task.FileBytes);
        using var package = new ExcelPackage(memoryStream);

        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.Dimension == null)
        {
            _channel.UpdateTaskStatus(task.TaskId, status =>
            {
                status.Status = BulkImportTaskStatus.Fallido;
                status.FinishedAtUtc = DateTime.UtcNow;
                status.ErrorMessages.Add("El archivo Excel no contiene hojas válidas o está vacío.");
            });
            return;
        }

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        if (rowCount < 2)
        {
            _channel.UpdateTaskStatus(task.TaskId, status =>
            {
                status.Status = BulkImportTaskStatus.Fallido;
                status.FinishedAtUtc = DateTime.UtcNow;
                status.ErrorMessages.Add("La planilla debe tener al menos una fila de encabezados y una fila de datos.");
            });
            return;
        }

        // Mapear encabezados de la fila 1
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= colCount; col++)
        {
            var headerValue = worksheet.Cells[1, col].Text?.Trim();
            if (!string.IsNullOrEmpty(headerValue) && !headerMap.ContainsKey(headerValue))
            {
                headerMap[headerValue] = col;
            }
        }

        int colBarcode = FindColumnIndex(headerMap, "Codigo", "Barcode", "EAN", "SKU", "CodigoBarra");
        int colName = FindColumnIndex(headerMap, "Nombre", "Name", "Producto", "Descripcion", "Title");
        int colPrice = FindColumnIndex(headerMap, "PrecioMayorista", "Precio", "WholesalePrice", "PrecioUnitario");
        int colBoxPrice = FindColumnIndex(headerMap, "PrecioBulto", "BoxPrice", "PrecioCaja");
        int colUnitsPerBox = FindColumnIndex(headerMap, "UnidadesPorBulto", "UnitsPerBox", "UnidadesBulto", "Bulto");
        int colStock = FindColumnIndex(headerMap, "Stock", "StockUnits", "Cantidad", "Inventario");
        int colBrand = FindColumnIndex(headerMap, "Marca", "Brand", "Fabricante");
        int colCategory = FindColumnIndex(headerMap, "Categoria", "Category", "Rubro");

        if (colBarcode == 0)
        {
            _channel.UpdateTaskStatus(task.TaskId, status =>
            {
                status.Status = BulkImportTaskStatus.Fallido;
                status.FinishedAtUtc = DateTime.UtcNow;
                status.ErrorMessages.Add("No se encontró la columna obligatoria de código de barras ('Codigo', 'Barcode' o 'EAN').");
            });
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        int totalDataRows = rowCount - 1;
        int processedRows = 0;
        int updatedCount = 0;
        int createdCount = 0;
        var errors = new List<string>();

        // Pre-cargar marcas y categorías existentes en memoria
        var brands = await context.Brands.ToDictionaryAsync(b => b.Name.ToLower(), b => b, stoppingToken);
        var categories = await context.Categories.ToDictionaryAsync(c => c.Name.ToLower(), c => c, stoppingToken);

        var defaultBrand = brands.Values.FirstOrDefault() ?? new Brand { Name = "General", IsActive = true };
        if (defaultBrand.Id == 0)
        {
            await context.Brands.AddAsync(defaultBrand, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            brands[defaultBrand.Name.ToLower()] = defaultBrand;
        }

        var defaultCategory = categories.Values.FirstOrDefault() ?? new Category { Name = "Varios", Slug = "varios", IsActive = true };
        if (defaultCategory.Id == 0)
        {
            await context.Categories.AddAsync(defaultCategory, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            categories[defaultCategory.Name.ToLower()] = defaultCategory;
        }

        for (int row = 2; row <= rowCount; row++)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var barcode = worksheet.Cells[row, colBarcode].Text?.Trim();
            if (string.IsNullOrWhiteSpace(barcode))
            {
                continue; // Fila vacía o sin código
            }

            try
            {
                var existingProduct = await context.Products
                    .FirstOrDefaultAsync(p => p.Barcode == barcode, stoppingToken);

                decimal? price = colPrice > 0 ? ParseDecimal(worksheet.Cells[row, colPrice].Text) : null;
                decimal? boxPrice = colBoxPrice > 0 ? ParseDecimal(worksheet.Cells[row, colBoxPrice].Text) : null;
                int? unitsPerBox = colUnitsPerBox > 0 ? ParseInt(worksheet.Cells[row, colUnitsPerBox].Text) : null;
                int? stock = colStock > 0 ? ParseInt(worksheet.Cells[row, colStock].Text) : null;
                string? name = colName > 0 ? worksheet.Cells[row, colName].Text?.Trim() : null;
                string? brandName = colBrand > 0 ? worksheet.Cells[row, colBrand].Text?.Trim() : null;
                string? categoryName = colCategory > 0 ? worksheet.Cells[row, colCategory].Text?.Trim() : null;

                if (existingProduct != null)
                {
                    // Actualización
                    if (price.HasValue && price.Value >= 0) existingProduct.WholesalePrice = price.Value;
                    if (boxPrice.HasValue && boxPrice.Value >= 0) existingProduct.BoxPrice = boxPrice.Value;
                    if (unitsPerBox.HasValue && unitsPerBox.Value > 0) existingProduct.UnitsPerBox = unitsPerBox.Value;
                    if (!string.IsNullOrEmpty(name)) existingProduct.Name = name;

                    if (stock.HasValue && stock.Value != existingProduct.StockUnits)
                    {
                        var delta = stock.Value - existingProduct.StockUnits;
                        existingProduct.StockUnits = stock.Value;

                        var movement = new StockMovement
                        {
                            Product = existingProduct,
                            MovementType = StockMovementType.ImportacionMasivaExcel,
                            Quantity = delta,
                            ResultingStock = stock.Value,
                            Reason = $"Ajuste masivo desde planilla Excel ({task.FileName})",
                            ResponsibleUserId = task.RequestedByUserId,
                            TimestampUtc = DateTime.UtcNow
                        };
                        await context.StockMovements.AddAsync(movement, stoppingToken);
                    }

                    existingProduct.UpdatedAtUtc = DateTime.UtcNow;
                    updatedCount++;
                }
                else
                {
                    // Alta de producto nuevo
                    if (string.IsNullOrEmpty(name))
                    {
                        errors.Add($"Fila {row}: Producto con código '{barcode}' no existe en catálogo y no especifica 'Nombre'.");
                        continue;
                    }

                    Brand brandEntity = defaultBrand;
                    if (!string.IsNullOrEmpty(brandName))
                    {
                        var key = brandName.ToLower();
                        if (!brands.TryGetValue(key, out var foundBrand))
                        {
                            foundBrand = new Brand { Name = brandName, IsActive = true };
                            await context.Brands.AddAsync(foundBrand, stoppingToken);
                            await context.SaveChangesAsync(stoppingToken);
                            brands[key] = foundBrand;
                        }
                        brandEntity = foundBrand;
                    }

                    Category categoryEntity = defaultCategory;
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        var key = categoryName.ToLower();
                        if (!categories.TryGetValue(key, out var foundCat))
                        {
                            foundCat = new Category { Name = categoryName, Slug = categoryName.ToLower().Replace(" ", "-"), IsActive = true };
                            await context.Categories.AddAsync(foundCat, stoppingToken);
                            await context.SaveChangesAsync(stoppingToken);
                            categories[key] = foundCat;
                        }
                        categoryEntity = foundCat;
                    }

                    var newProduct = new Product
                    {
                        Barcode = barcode,
                        Name = name,
                        BrandId = brandEntity.Id,
                        CategoryId = categoryEntity.Id,
                        WholesalePrice = price ?? 0m,
                        BoxPrice = boxPrice ?? (price.HasValue ? price.Value * (unitsPerBox ?? 1) : 0m),
                        UnitsPerBox = unitsPerBox ?? 1,
                        StockUnits = Math.Max(0, stock ?? 0),
                        SafetyStock = 10,
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    await context.Products.AddAsync(newProduct, stoppingToken);

                    if (newProduct.StockUnits > 0)
                    {
                        var movement = new StockMovement
                        {
                            Product = newProduct,
                            MovementType = StockMovementType.ImportacionMasivaExcel,
                            Quantity = newProduct.StockUnits,
                            ResultingStock = newProduct.StockUnits,
                            Reason = $"Carga inicial desde planilla Excel ({task.FileName})",
                            ResponsibleUserId = task.RequestedByUserId,
                            TimestampUtc = DateTime.UtcNow
                        };
                        await context.StockMovements.AddAsync(movement, stoppingToken);
                    }

                    createdCount++;
                }

                processedRows++;

                // Persistir lotes cada 50 registros para optimizar transacciones
                if (processedRows % 50 == 0)
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Fila {row} (Código: {barcode}): {ex.Message}");
            }
        }

        await context.SaveChangesAsync(stoppingToken);

        _channel.UpdateTaskStatus(task.TaskId, status =>
        {
            status.Status = BulkImportTaskStatus.Completado;
            status.TotalRows = totalDataRows;
            status.ProcessedRows = processedRows;
            status.UpdatedProductsCount = updatedCount;
            status.CreatedProductsCount = createdCount;
            status.ErrorCount = errors.Count;
            status.ErrorMessages = errors;
            status.FinishedAtUtc = DateTime.UtcNow;
        });

        _logger.LogInformation("Tarea de importación {TaskId} finalizada. Procesados: {Processed}, Actualizados: {Updated}, Creados: {Created}, Errores: {Errors}",
            task.TaskId, processedRows, updatedCount, createdCount, errors.Count);
    }

    private static int FindColumnIndex(Dictionary<string, int> headerMap, params string[] candidateNames)
    {
        foreach (var candidate in candidateNames)
        {
            if (headerMap.TryGetValue(candidate, out var index))
            {
                return index;
            }
        }
        return 0;
    }

    private static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = text.Replace("$", "").Replace(" ", "").Trim();
        if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ||
            decimal.TryParse(clean, NumberStyles.Any, new CultureInfo("es-AR"), out value))
        {
            return value;
        }
        return null;
    }

    private static int? ParseInt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (int.TryParse(text.Trim(), out var value))
        {
            return value;
        }
        return null;
    }
}
