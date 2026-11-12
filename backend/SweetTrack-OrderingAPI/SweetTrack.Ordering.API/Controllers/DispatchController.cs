using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetTrack.Ordering.Application.DTOs;
using SweetTrack.Ordering.Application.Interfaces;
using SweetTrack.Ordering.Domain.Exceptions;

namespace SweetTrack.Ordering.API.Controllers;

[ApiController]
[Route("api/dispatch")]
[Authorize(Roles = "Admin")]
public class DispatchController : ControllerBase
{
    private readonly IOrderWorkflowService _workflowService;
    private readonly IPdfGeneratorService _pdfService;
    private readonly ILogger<DispatchController> _logger;

    public DispatchController(
        IOrderWorkflowService workflowService,
        IPdfGeneratorService pdfService,
        ILogger<DispatchController> logger)
    {
        _workflowService = workflowService;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna el tablero Kanban de despacho con los pedidos activos agrupados por etapa logística.
    /// Exclusivo para administradores y operadores de depósito.
    /// </summary>
    [HttpGet("board")]
    [ProducesResponseType(typeof(DispatchBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDispatchBoard(CancellationToken cancellationToken)
    {
        var board = await _workflowService.GetDispatchBoardAsync(cancellationToken);
        return Ok(board);
    }

    /// <summary>
    /// Genera y descarga en streaming el remito comercial de entrega en PDF formato A4 con código de barras Code128.
    /// </summary>
    [HttpGet("orders/{id:guid}/remito-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadWaybillPdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateWaybillPdfAsync(id, cancellationToken);
            var filename = $"Remito-SweetTrack-{id.ToString()[..8].ToUpper()}.pdf";
            return File(pdfBytes, "application/pdf", filename);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning("Intento de generar remito para orden inexistente: {OrderId}", id);
            return NotFound(new { message = ex.Message });
        }
    }
}
