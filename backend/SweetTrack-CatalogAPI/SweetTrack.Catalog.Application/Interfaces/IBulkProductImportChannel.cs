using SweetTrack.Catalog.Application.DTOs.BulkImport;

namespace SweetTrack.Catalog.Application.Interfaces;

public interface IBulkProductImportChannel
{
    ValueTask EnqueueAsync(BulkImportTask task, CancellationToken cancellationToken = default);
    IAsyncEnumerable<BulkImportTask> ReadAllAsync(CancellationToken cancellationToken = default);
    BulkImportStatusDto? GetTaskStatus(string taskId);
    void SetTaskStatus(BulkImportStatusDto status);
    void UpdateTaskStatus(string taskId, Action<BulkImportStatusDto> updateAction);
}

