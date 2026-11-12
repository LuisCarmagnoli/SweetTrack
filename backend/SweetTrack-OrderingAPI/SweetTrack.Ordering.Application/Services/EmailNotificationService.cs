using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SweetTrack.Ordering.Application.Interfaces;

namespace SweetTrack.Ordering.Application.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(
        string recipientEmail,
        string kioskName,
        string orderNumber,
        decimal totalAmount,
        byte[]? waybillPdf = null,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "SweetTrack Mayorista";
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "pedidos@sweettrack.com";

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(kioskName, recipientEmail));
        message.Subject = $"SweetTrack - Confirmación de Pedido {orderNumber}";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <h2 style='color: #4f46e5;'>¡Gracias por tu pedido, {kioskName}!</h2>
                    <p>Hemos recibido tu pedido <strong>{orderNumber}</strong> por un importe total de <strong>${totalAmount:N2}</strong>.</p>
                    <p>El equipo de logística comenzará el proceso de preparación en depósito a la brevedad.</p>
                    <hr style='border: 1px solid #e5e7eb;' />
                    <p style='color: #6b7280; font-size: 12px;'>SweetTrack B2B - Plataforma Mayorista de Golosinas.</p>
                </div>"
        };

        if (waybillPdf != null && waybillPdf.Length > 0)
        {
            builder.Attachments.Add($"Remito-{orderNumber}.pdf", waybillPdf, new ContentType("application", "pdf"));
        }

        message.Body = builder.ToMessageBody();

        await SendOrLogEmailAsync(message, recipientEmail, orderNumber, cancellationToken);
    }

    public async Task SendStatusChangedEmailAsync(
        string recipientEmail,
        string kioskName,
        string orderNumber,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "SweetTrack Mayorista";
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "pedidos@sweettrack.com";

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(kioskName, recipientEmail));
        message.Subject = $"SweetTrack - Actualización de Pedido {orderNumber}";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <h2 style='color: #4f46e5;'>Actualización de tu Pedido</h2>
                    <p>Hola <strong>{kioskName}</strong>,</p>
                    <p>Te informamos que tu pedido <strong>{orderNumber}</strong> ha cambiado al estado: <span style='font-weight: bold; color: #10b981;'>{newStatus}</span>.</p>
                    <p>Puedes seguir el avance en tiempo real desde tu panel de usuario en SweetTrack.</p>
                    <hr style='border: 1px solid #e5e7eb;' />
                    <p style='color: #6b7280; font-size: 12px;'>SweetTrack B2B - Plataforma Mayorista de Golosinas.</p>
                </div>"
        };

        message.Body = builder.ToMessageBody();

        await SendOrLogEmailAsync(message, recipientEmail, orderNumber, cancellationToken);
    }

    private async Task SendOrLogEmailAsync(
        MimeMessage message,
        string recipient,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var server = _configuration["SmtpSettings:Server"];
        var port = int.TryParse(_configuration["SmtpSettings:Port"], out var p) ? p : 587;
        var user = _configuration["SmtpSettings:Username"];
        var pass = _configuration["SmtpSettings:Password"];

        try
        {
            if (string.IsNullOrWhiteSpace(server) || server == "localhost")
            {
                _logger.LogInformation("[SMTP Mock] Correo para {Recipient} (Pedido {OrderNumber}): '{Subject}' registrado en logs de desarrollo.", recipient, orderNumber, message.Subject);
                return;
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.Auto, cancellationToken);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await client.AuthenticateAsync(user, pass, cancellationToken);
            }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Correo enviado con éxito a {Recipient} para pedido {OrderNumber}", recipient, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Aviso: No se pudo despachar el correo SMTP a {Recipient} para el pedido {OrderNumber}. Continuando flujo.", recipient, orderNumber);
        }
    }
}
