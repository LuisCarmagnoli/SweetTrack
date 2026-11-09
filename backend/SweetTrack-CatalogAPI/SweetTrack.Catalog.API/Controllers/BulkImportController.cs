using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetTrack.Catalog.Application.DTOs.BulkImport;
using SweetTrack.Catalog.Application.Interfaces;

namespace SweetTrack.Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/bulk-import")]
public class BulkImportController : ControllerBase
{
    private const long MaxFileSizeBytes = 15 * 1024 * 1024; // 15 MB
    private readonly IBulkProductImportChannel _channel;

    public BulkImportController(IBulkProductImportChannel channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// Sube una planilla Excel (.xlsx) para encolar la importación masiva no bloqueante de productos y precios.
    /// </summary>
    [HttpPost("excel")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BulkImportResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadExcel(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "El archivo Excel es obligatorio y no puede estar vacío." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = $"El tamaño del archivo supera el límite máximo permitido de {MaxFileSizeBytes / (1024 * 1024)} MB." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return BadRequest(new { message = "Formato no válido. Solo se admiten planillas en formato Excel OpenXML (.xlsx)." });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        var task = new BulkImportTask
        {
            FileName = file.FileName,
            FileBytes = memoryStream.ToArray(),
            RequestedByUserId = userId,
            QueuedAtUtc = DateTime.UtcNow
        };

        await _channel.EnqueueAsync(task, cancellationToken);

        var response = new BulkImportResponseDto
        {
            TaskId = task.TaskId,
            Message = "Archivo recibido correctamente. El procesamiento ha sido encolado en segundo plano.",
            Status = BulkImportTaskStatus.EnCola
        };

        return AcceptedAtAction(nameof(GetStatus), new { taskId = task.TaskId }, response);
    }

    /// <summary>
    /// Consulta el estado de procesamiento y los resultados de una tarea de importación masiva.
    /// </summary>
    [HttpGet("status/{taskId}")]
    [Authorize]
    [ProducesResponseType(typeof(BulkImportStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetStatus(string taskId)
    {
        var status = _channel.GetTaskStatus(taskId);
        if (status == null)
        {
            return NotFound(new { message = $"No se encontró ninguna tarea de importación con ID '{taskId}'." });
        }

        return Ok(status);
    }
}
