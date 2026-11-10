using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetTrack.Catalog.Application.DTOs.Categories;
using SweetTrack.Catalog.Application.Interfaces;

namespace SweetTrack.Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Devuelve el catálogo de categorías comerciales de productos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var categories = await _catalogService.GetCategoriesAsync(onlyActive, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Registra una nueva categoría de producto. Requiere rol Administrador.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _catalogService.CreateCategoryAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }
}

