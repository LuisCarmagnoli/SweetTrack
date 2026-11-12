using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SweetTrack.Ordering.Application.DTOs;
using SweetTrack.Ordering.Application.Options;
using SweetTrack.Ordering.Application.Services;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;
using Xunit;

namespace SweetTrack.Ordering.Tests.Application;

public class OrderWorkflowServiceTests
{
    private static OrderingDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseInMemoryDatabase(databaseName: $"SweetTrack_Orders_Test_{Guid.NewGuid()}")
            .Options;

        return new OrderingDbContext(options);
    }

    private static IOptions<BusinessRulesOptions> CreateOptions(decimal minimum = 50000m)
    {
        return Options.Create(new BusinessRulesOptions
        {
            MinimumOrderAmount = minimum,
            AbandonedCartExpirationHours = 24
        });
    }

    [Fact]
    public async Task CreateOrderAsync_WhenBelowMinimumThreshold_ThrowsMinimumOrderAmountNotMetException()
    {
        using var context = CreateInMemoryDbContext();
        var service = new OrderWorkflowService(context, CreateOptions(50000m), NullLogger<OrderWorkflowService>.Instance);

        var request = new CreateOrderRequestDto(
            RazonSocialKiosco: "Kiosco Pepe",
            CuitKiosco: "20-11223344-5",
            DireccionEntrega: "Av. Rivadavia 100",
            PaymentMethod: PaymentMethod.EfectivoContraEntrega,
            Items: new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), "77912345", "Chicle", 5, 1, 100m) // 5 * 100 = 500 (+IVA = 605) < 50.000
            },
            DeliveryNotes: null);

        var ex = await Assert.ThrowsAsync<MinimumOrderAmountNotMetException>(() =>
            service.CreateOrderAsync("kiosk-user-1", request));

        Assert.True(ex.CurrentAmount < ex.MinimumRequired);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_PersistsOrderWithItemsAndAuditHistory()
    {
        using var context = CreateInMemoryDbContext();
        var service = new OrderWorkflowService(context, CreateOptions(50000m), NullLogger<OrderWorkflowService>.Instance);

        var request = new CreateOrderRequestDto(
            RazonSocialKiosco: "Kiosco San Martín",
            CuitKiosco: "20-33445566-7",
            DireccionEntrega: "Calle Falsa 123",
            PaymentMethod: PaymentMethod.CuentaCorriente,
            Items: new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), "77911111", "Caja Bon o Bon", 10, 10, 5000m) // 50.000 + IVA (10.500) = 60.500 >= 50.000
            },
            DeliveryNotes: "Entregar por la tarde");

        var response = await service.CreateOrderAsync("kiosk-user-2", request);

        Assert.NotNull(response);
        Assert.StartsWith("ORD-", response.OrderNumber);
        Assert.Equal(50000m, response.Subtotal);
        Assert.Equal(10500m, response.VatAmount);
        Assert.Equal(60500m, response.TotalAmount);
        Assert.Equal(OrderStatus.PendienteConfirmacion, response.Status);

        // Verificar persistencia en base de datos
        var persistedOrder = await context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == response.Id);

        Assert.NotNull(persistedOrder);
        Assert.Single(persistedOrder.Items);
        Assert.Single(persistedOrder.StatusHistories);
        Assert.Equal("kiosk-user-2", persistedOrder.StatusHistories.First().UsuarioModificadorId);
    }

    [Fact]
    public async Task TransitionStatusAsync_WhenValid_TransitionsAndPersists()
    {
        using var context = CreateInMemoryDbContext();
        var service = new OrderWorkflowService(context, CreateOptions(50000m), NullLogger<OrderWorkflowService>.Instance);

        var order = new Order("ORD-2026-000099", "kiosk-user-1", "Kiosco Centro", "20-11111111-1", "Av. Mayo 50", PaymentMethod.CuentaCorriente);
        order.AddItem(new OrderItem(Guid.NewGuid(), "779123", "Golosina", 50, 5, 1000m));
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var request = new StatusTransitionRequestDto(OrderStatus.EnPreparacion, "Iniciando empaque");
        var result = await service.TransitionStatusAsync(order.Id, request, "admin-operator", "Admin");

        Assert.Equal(OrderStatus.EnPreparacion, result.Status);
        Assert.Contains(result.StatusHistory, h => h.NewStatus == OrderStatus.EnPreparacion);
    }

    [Fact]
    public async Task GetDispatchBoardAsync_ReturnsActiveOrdersGrouped()
    {
        using var context = CreateInMemoryDbContext();
        var service = new OrderWorkflowService(context, CreateOptions(50000m), NullLogger<OrderWorkflowService>.Instance);

        var orderPrep = new Order("ORD-001", "u1", "K1", "20-1", "Dir 1", PaymentMethod.CuentaCorriente) { EstadoActual = OrderStatus.EnPreparacion };
        var orderReady = new Order("ORD-002", "u2", "K2", "20-2", "Dir 2", PaymentMethod.CuentaCorriente) { EstadoActual = OrderStatus.ListoParaDespacho };
        var orderRoute = new Order("ORD-003", "u3", "K3", "20-3", "Dir 3", PaymentMethod.CuentaCorriente) { EstadoActual = OrderStatus.EnReparto };
        var orderDelivered = new Order("ORD-004", "u4", "K4", "20-4", "Dir 4", PaymentMethod.CuentaCorriente) { EstadoActual = OrderStatus.Entregado };

        context.Orders.AddRange(orderPrep, orderReady, orderRoute, orderDelivered);
        await context.SaveChangesAsync();

        var board = await service.GetDispatchBoardAsync();

        Assert.Single(board.EnPreparacion);
        Assert.Single(board.ListoParaDespacho);
        Assert.Single(board.EnReparto);
    }
}
