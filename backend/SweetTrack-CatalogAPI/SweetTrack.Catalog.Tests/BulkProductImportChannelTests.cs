using SweetTrack.Catalog.Application.Channels;
using SweetTrack.Catalog.Application.DTOs.BulkImport;

namespace SweetTrack.Catalog.Tests;

public class BulkProductImportChannelTests
{
    [Fact]
    public async Task Channel_EnqueueAndRead_TransfersTaskAndTracksStatus()
    {
        // Arrange
        var channel = new BulkProductImportChannel(capacity: 10);
        var task = new BulkImportTask
        {
            TaskId = "task-test-123",
            FileName = "lista_precios_arcor.xlsx",
            FileBytes = [0x50, 0x4B, 0x03, 0x04],
            RequestedByUserId = "admin-1"
        };

        // Act - Enqueue
        await channel.EnqueueAsync(task);

        // Assert - Initial Status
        var status = channel.GetTaskStatus("task-test-123");
        Assert.NotNull(status);
        Assert.Equal("task-test-123", status.TaskId);
        Assert.Equal("lista_precios_arcor.xlsx", status.FileName);
        Assert.Equal(BulkImportTaskStatus.EnCola, status.Status);

        // Act - Update Status
        channel.UpdateTaskStatus("task-test-123", s =>
        {
            s.Status = BulkImportTaskStatus.Procesando;
            s.ProcessedRows = 100;
            s.UpdatedProductsCount = 85;
            s.CreatedProductsCount = 15;
        });

        // Assert - Updated Status
        var updatedStatus = channel.GetTaskStatus("task-test-123");
        Assert.NotNull(updatedStatus);
        Assert.Equal(BulkImportTaskStatus.Procesando, updatedStatus.Status);
        Assert.Equal(100, updatedStatus.ProcessedRows);
        Assert.Equal(85, updatedStatus.UpdatedProductsCount);
        Assert.Equal(15, updatedStatus.CreatedProductsCount);
    }

    [Fact]
    public void Channel_GetTaskStatus_ReturnsNull_ForNonExistentTask()
    {
        var channel = new BulkProductImportChannel();
        var status = channel.GetTaskStatus("non-existent-task");
        Assert.Null(status);
    }
}
