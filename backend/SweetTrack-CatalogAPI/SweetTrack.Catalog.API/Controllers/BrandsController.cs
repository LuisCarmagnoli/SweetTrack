using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetTrack.Catalog.Application.DTOs.Brands;
using SweetTrack.Catalog.Application.Interfaces;

namespace SweetTrack.Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/brands")]
public class BrandsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public BrandsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Devuelve las marcas comerciales mayoristas disponibles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrandDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var brands = await _catalogService.GetBrandsAsync(onlyActive, cancellationToken);
        return Ok(brands);
    }

    /// <summary>
    /// Registra una nueva marca de fabricante o distribuidor. Requiere rol Administrador.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var brand = await _catalogService.CreateBrandAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = brand.Id }, brand);
    }
}

