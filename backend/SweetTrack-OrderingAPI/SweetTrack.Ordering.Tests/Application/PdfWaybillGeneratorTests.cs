using Microsoft.EntityFrameworkCore;
using SweetTrack.Ordering.Application.Services;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;
using Xunit;

namespace SweetTrack.Ordering.Tests.Application;

public class PdfWaybillGeneratorTests
{
    private static OrderingDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseInMemoryDatabase(databaseName: $"SweetTrack_Pdf_Test_{Guid.NewGuid()}")
            .Options;

        return new OrderingDbContext(options);
    }

    [Fact]
    public async Task GenerateWaybillPdfAsync_WhenOrderExists_ReturnsValidPdfBytes()
    {
        using var context = CreateInMemoryDbContext();
        var generator = new PdfWaybillGenerator(context);

        var order = new Order("ORD-2026-000555", "kiosk-user-1", "Kiosco Dulce Tentación", "20-99887766-5", "Calle 10 N° 500", PaymentMethod.CuentaCorriente);
        order.AddItem(new OrderItem(Guid.NewGuid(), "7791234567890", "Alfajor Chocolate x24", 48, 2, 850m));
        order.AddItem(new OrderItem(Guid.NewGuid(), "7790987654321", "Caramelos Masticables Frutales", 100, 5, 200m));
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var pdfBytes = await generator.GenerateWaybillPdfAsync(order.Id);

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "El PDF generado debe contener bytes significativos.");

        // Un archivo PDF válido siempre inicia con '%PDF-'
        var pdfHeader = System.Text.Encoding.ASCII.GetString(pdfBytes[..5]);
        Assert.Equal("%PDF-", pdfHeader);
    }

    [Fact]
    public async Task GenerateWaybillPdfAsync_WhenOrderNotFound_ThrowsOrderNotFoundException()
    {
        using var context = CreateInMemoryDbContext();
        var generator = new PdfWaybillGenerator(context);

        var randomId = Guid.NewGuid();
        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            generator.GenerateWaybillPdfAsync(randomId));
    }
}
