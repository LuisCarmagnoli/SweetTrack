using System.Collections.Concurrent;
using System.Threading.Channels;
using SweetTrack.Catalog.Application.DTOs.BulkImport;
using SweetTrack.Catalog.Application.Interfaces;

namespace SweetTrack.Catalog.Application.Channels;

public class BulkProductImportChannel : IBulkProductImportChannel
{
    private readonly Channel<BulkImportTask> _channel;
    private readonly ConcurrentDictionary<string, BulkImportStatusDto> _statusCache = new();

    public BulkProductImportChannel(int capacity = 50)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<BulkImportTask>(options);
    }

    public async ValueTask EnqueueAsync(BulkImportTask task, CancellationToken cancellationToken = default)
    {
        var status = new BulkImportStatusDto
        {
            TaskId = task.TaskId,
            FileName = task.FileName,
            Status = BulkImportTaskStatus.EnCola,
            StartedAtUtc = null
        };

        _statusCache[task.TaskId] = status;
        await _channel.Writer.WriteAsync(task, cancellationToken);
    }

    public IAsyncEnumerable<BulkImportTask> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    public BulkImportStatusDto? GetTaskStatus(string taskId)
    {
        _statusCache.TryGetValue(taskId, out var status);
        return status;
    }

    public void SetTaskStatus(BulkImportStatusDto status)
    {
        _statusCache[status.TaskId] = status;
    }

    public void UpdateTaskStatus(string taskId, Action<BulkImportStatusDto> updateAction)
    {
        if (_statusCache.TryGetValue(taskId, out var status))
        {
            lock (status)
            {
                updateAction(status);
            }
        }
    }
}
