using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetTrack.Catalog.Application.DTOs.Common;
using SweetTrack.Catalog.Application.DTOs.Products;
using SweetTrack.Catalog.Application.DTOs.Stock;
using SweetTrack.Catalog.Application.Interfaces;
using SweetTrack.Catalog.Domain.Exceptions;

namespace SweetTrack.Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IStockService _stockService;

    public ProductsController(ICatalogService catalogService, IStockService stockService)
    {
        _catalogService = catalogService;
        _stockService = stockService;
    }

    /// <summary>
    /// Consulta el listado paginado y filtrable de productos del catálogo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] CatalogFilterParamsDto filter, CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetProductsAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle técnico y comercial de un producto por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _catalogService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"El producto con ID '{id}' no fue encontrado." });
        }

        return Ok(product);
    }

    /// <summary>
    /// Obtiene un producto por su código de barras o EAN.
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBarcode(string barcode, CancellationToken cancellationToken)
    {
        var product = await _catalogService.GetProductByBarcodeAsync(barcode, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"No se encontró ningún producto con código de barras '{barcode}'." });
        }

        return Ok(product);
    }

    /// <summary>
    /// Registra un nuevo producto en el catálogo mayorista. Requiere autenticación.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var created = await _catalogService.CreateProductAsync(dto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DuplicateBarcodeException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza los datos comerciales y de empaque de un producto.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _catalogService.UpdateProductAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DuplicateBarcodeException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Realiza una baja lógica de un producto en el catálogo.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _catalogService.DeleteProductAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"El producto con ID '{id}' no fue encontrado." });
        }

        return NoContent();
    }

    /// <summary>
    /// Realiza un ajuste manual de stock (ingreso o egreso) registrando trazabilidad inmutable.
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    [Authorize]
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var movement = await _stockService.AdjustStockAsync(id, dto, userId, cancellationToken);
            return Ok(movement);
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial reciente de movimientos de stock de un producto.
    /// </summary>
    [HttpGet("{id:int}/stock-movements")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<StockMovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockMovements(int id, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var movements = await _stockService.GetProductStockHistoryAsync(id, limit, cancellationToken);
        return Ok(movements);
    }
}
