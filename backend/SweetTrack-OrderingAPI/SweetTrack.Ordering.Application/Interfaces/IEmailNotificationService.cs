namespace SweetTrack.Ordering.Application.Interfaces;

public interface IEmailNotificationService
{
    Task SendOrderConfirmationEmailAsync(
        string recipientEmail,
        string kioskName,
        string orderNumber,
        decimal totalAmount,
        byte[]? waybillPdf = null,
        CancellationToken cancellationToken = default);

    Task SendStatusChangedEmailAsync(
        string recipientEmail,
        string kioskName,
        string orderNumber,
        string newStatus,
        CancellationToken cancellationToken = default);
}
