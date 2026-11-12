namespace SweetTrack.Ordering.Application.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateWaybillPdfAsync(Guid orderId, CancellationToken cancellationToken = default);
}
