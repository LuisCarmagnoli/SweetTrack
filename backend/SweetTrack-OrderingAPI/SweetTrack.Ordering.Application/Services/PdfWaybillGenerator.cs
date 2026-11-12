using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SweetTrack.Ordering.Application.Interfaces;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Exceptions;

namespace SweetTrack.Ordering.Application.Services;

public class PdfWaybillGenerator : IPdfGeneratorService
{
    private readonly OrderingDbContext _context;

    public PdfWaybillGenerator(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateWaybillPdfAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.DeliveryRoute)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        using var document = new PdfDocument();
        document.Info.Title = $"Remito Comercial - {order.NumeroPedido}";
        document.Info.Author = "SweetTrack Mayorista B2B";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        // Fuentes
        var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
        var fontSubtitle = new XFont("Arial", 10, XFontStyle.Regular);
        var fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
        var fontBody = new XFont("Arial", 9, XFontStyle.Regular);
        var fontBodyBold = new XFont("Arial", 9, XFontStyle.Bold);
        var fontSmall = new XFont("Arial", 8, XFontStyle.Regular);
        var fontBarcode = new XFont("Courier New", 12, XFontStyle.Bold);

        // Pinceles y Plumas
        var brushBlack = XBrushes.Black;
        var brushGray = XBrushes.DarkGray;
        var brushHeaderBg = new XSolidBrush(XColor.FromArgb(240, 240, 245));
        var penLine = new XPen(XColor.FromArgb(200, 200, 210), 1);
        var penDark = new XPen(XColors.Black, 1.5);

        double y = 40;

        // 1. Encabezado de la Empresa y Remito
        gfx.DrawString("SWEETTRACK B2B", fontTitle, brushBlack, new XPoint(40, y));
        gfx.DrawString("Distribuidora Mayorista de Golosinas y Alimentos", fontSubtitle, brushGray, new XPoint(40, y + 16));
        gfx.DrawString("Av. Corrientes 4500, CABA | CUIT: 30-71829384-5 | IVA Resp. Inscripto", fontSmall, brushGray, new XPoint(40, y + 30));

        // Cuadro Tipo Comprobante
        gfx.DrawRectangle(penDark, 275, y - 10, 45, 45);
        var fontLetter = new XFont("Arial", 22, XFontStyle.Bold);
        gfx.DrawString("R", fontLetter, brushBlack, new XPoint(288, y + 22));
        gfx.DrawString("COD. 91", fontSmall, brushBlack, new XPoint(282, y + 32));

        // Datos del Remito (Derecha)
        gfx.DrawString("REMITO DE ENTREGA", fontHeader, brushBlack, new XPoint(360, y));
        gfx.DrawString($"N°: {order.NumeroPedido}", fontBodyBold, brushBlack, new XPoint(360, y + 16));
        gfx.DrawString($"Fecha: {order.FechaCreacionUtc:dd/MM/yyyy HH:mm} UTC", fontBody, brushBlack, new XPoint(360, y + 30));
        gfx.DrawString($"Estado: {order.EstadoActual}", fontBodyBold, brushBlack, new XPoint(360, y + 44));

        y += 65;
        gfx.DrawLine(penDark, 40, y, 555, y);
        y += 15;

        // 2. Datos del Cliente / Kiosco
        gfx.DrawRectangle(brushHeaderBg, 40, y - 5, 515, 60);
        gfx.DrawRectangle(penLine, 40, y - 5, 515, 60);

        gfx.DrawString("DESTINATARIO (CLIENTE B2B)", fontHeader, brushBlack, new XPoint(50, y + 10));
        gfx.DrawString($"Razón Social: {order.RazonSocialKiosco}", fontBodyBold, brushBlack, new XPoint(50, y + 25));
        gfx.DrawString($"CUIT: {order.CuitKiosco}", fontBody, brushBlack, new XPoint(50, y + 40));

        gfx.DrawString($"Dirección: {order.DireccionEntrega}", fontBody, brushBlack, new XPoint(300, y + 25));
        gfx.DrawString($"Método Pago: {order.MetodoPago}", fontBody, brushBlack, new XPoint(300, y + 40));

        y += 75;

        // 3. Tabla de Productos
        gfx.DrawRectangle(brushHeaderBg, 40, y, 515, 20);
        gfx.DrawRectangle(penLine, 40, y, 515, 20);

        gfx.DrawString("CÓDIGO", fontHeader, brushBlack, new XPoint(45, y + 14));
        gfx.DrawString("DESCRIPCIÓN DEL PRODUCTO", fontHeader, brushBlack, new XPoint(125, y + 14));
        gfx.DrawString("BULTOS", fontHeader, brushBlack, new XPoint(340, y + 14));
        gfx.DrawString("UNIDADES", fontHeader, brushBlack, new XPoint(395, y + 14));
        gfx.DrawString("P. UNIT", fontHeader, brushBlack, new XPoint(455, y + 14));
        gfx.DrawString("TOTAL", fontHeader, brushBlack, new XPoint(510, y + 14));

        y += 20;

        foreach (var item in order.Items)
        {
            gfx.DrawLine(penLine, 40, y + 18, 555, y + 18);

            gfx.DrawString(item.CodigoBarrasSnapshot, fontSmall, brushBlack, new XPoint(45, y + 13));

            // Truncar nombre si es muy largo
            var name = item.NombreProductoSnapshot.Length > 36 
                ? item.NombreProductoSnapshot[..33] + "..." 
                : item.NombreProductoSnapshot;
            gfx.DrawString(name, fontBody, brushBlack, new XPoint(125, y + 13));

            gfx.DrawString(item.CantidadBultos.ToString(), fontBody, brushBlack, new XPoint(355, y + 13));
            gfx.DrawString(item.CantidadUnidades.ToString(), fontBody, brushBlack, new XPoint(410, y + 13));
            gfx.DrawString($"${item.PrecioUnitarioSnapshot:N2}", fontBody, brushBlack, new XPoint(455, y + 13));
            gfx.DrawString($"${item.SubtotalLinea:N2}", fontBodyBold, brushBlack, new XPoint(510, y + 13));

            y += 20;
        }

        y += 15;

        // 4. Totales y Código de Barras Logístico
        gfx.DrawRectangle(brushHeaderBg, 340, y, 215, 65);
        gfx.DrawRectangle(penLine, 340, y, 215, 65);

        gfx.DrawString($"Subtotal Neto:", fontBody, brushBlack, new XPoint(350, y + 18));
        gfx.DrawString($"${order.Subtotal:N2}", fontBodyBold, brushBlack, new XPoint(480, y + 18));

        gfx.DrawString($"IVA (21%):", fontBody, brushBlack, new XPoint(350, y + 36));
        gfx.DrawString($"${order.MontoIva:N2}", fontBodyBold, brushBlack, new XPoint(480, y + 36));

        gfx.DrawString($"TOTAL:", fontHeader, brushBlack, new XPoint(350, y + 54));
        gfx.DrawString($"${order.MontoTotal:N2}", fontHeader, brushBlack, new XPoint(475, y + 54));

        // Código de barras simulado para lectura en depósito
        var barcodeX = 40.0;
        gfx.DrawString("CÓDIGO DE SEGUIMIENTO LOGÍSTICO", fontSmall, brushGray, new XPoint(barcodeX, y + 12));
        DrawBarcodeBars(gfx, barcodeX, y + 18, order.NumeroPedido);
        gfx.DrawString($"* {order.NumeroPedido} *", fontBarcode, brushBlack, new XPoint(barcodeX + 15, y + 60));

        y += 90;

        // 5. Conformidad de Entrega (Firma)
        gfx.DrawRectangle(penLine, 40, y, 515, 75);
        gfx.DrawString("CONFORMIDAD DE RECEPCIÓN", fontSmall, brushGray, new XPoint(50, y + 15));
        gfx.DrawLine(penLine, 50, y + 50, 260, y + 50);
        gfx.DrawString("Firma y Aclaración del Receptor", fontSmall, brushBlack, new XPoint(50, y + 62));

        gfx.DrawLine(penLine, 320, y + 50, 530, y + 50);
        gfx.DrawString("DNI / Fecha y Hora de Entrega", fontSmall, brushBlack, new XPoint(320, y + 62));

        using var ms = new MemoryStream();
        document.Save(ms, false);
        return ms.ToArray();
    }

    /// <summary>
    /// Genera la representación visual de barras tipo Code128 para lectura con pistolas lectoras en depósito.
    /// </summary>
    private static void DrawBarcodeBars(XGraphics gfx, double startX, double startY, string text)
    {
        var hash = Math.Abs(text.GetHashCode());
        var x = startX;
        var barBrush = XBrushes.Black;

        // Patrón pseudo-code128 basado en el hash del pedido
        for (int i = 0; i < 48; i++)
        {
            var isWide = ((hash >> (i % 31)) & 1) == 1;
            var width = isWide ? 3.0 : 1.5;
            
            if (i % 2 == 0)
            {
                gfx.DrawRectangle(barBrush, x, startY, width, 30);
            }
            x += width + 1.2;
        }
    }
}

