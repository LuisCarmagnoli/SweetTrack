namespace SweetTrack.Catalog.Application.DTOs.BulkImport;

public enum BulkImportTaskStatus
{
    EnCola = 1,
    Procesando = 2,
    Completado = 3,
    Fallido = 4
}

public class BulkImportTask
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = string.Empty;
    public byte[] FileBytes { get; set; } = [];
    public string? RequestedByUserId { get; set; }
    public DateTime QueuedAtUtc { get; set; } = DateTime.UtcNow;
}

public class BulkImportStatusDto
{
    public string TaskId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public BulkImportTaskStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int UpdatedProductsCount { get; set; }
    public int CreatedProductsCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
}

public class BulkImportResponseDto
{
    public string TaskId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public BulkImportTaskStatus Status { get; set; } = BulkImportTaskStatus.EnCola;
}

