using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;
using Xunit;

namespace SweetTrack.Ordering.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void AddItem_RecalculatesTotals_WithCorrectIVA()
    {
        var order = new Order("ORD-2026-0001", "user-123", "Kiosco Central", "20-12345678-9", "Av. Corrientes 1234", PaymentMethod.CuentaCorriente);

        var item1 = new OrderItem(Guid.NewGuid(), "7791234567890", "Alfajor Havanna", 10, 1, 1000m); // 10.000
        var item2 = new OrderItem(Guid.NewGuid(), "7790987654321", "Caramelos Sugus", 5, 1, 2000m);   // 10.000

        order.AddItem(item1);
        order.AddItem(item2);

        Assert.Equal(20000m, order.Subtotal);
        Assert.Equal(4200m, order.MontoIva);
        Assert.Equal(24200m, order.MontoTotal);
        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void TransitionTo_ValidTransition_UpdatesStatusAndAddsHistory()
    {
        var order = new Order("ORD-2026-0001", "user-123", "Kiosco Central", "20-12345678-9", "Av. Corrientes 1234", PaymentMethod.CuentaCorriente);

        order.TransitionTo(OrderStatus.EnPreparacion, "admin-user", "Admin", "Comenzando armado en depósito");

        Assert.Equal(OrderStatus.EnPreparacion, order.EstadoActual);
        Assert.Single(order.StatusHistories);

        var history = order.StatusHistories.First();
        Assert.Equal(OrderStatus.PendienteConfirmacion, history.EstadoAnterior);
        Assert.Equal(OrderStatus.EnPreparacion, history.EstadoNuevo);
        Assert.Equal("admin-user", history.UsuarioModificadorId);
        Assert.Equal("Comenzando armado en depósito", history.NotasMotivo);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ThrowsExceptionAndDoesNotModifyState()
    {
        var order = new Order("ORD-2026-0001", "user-123", "Kiosco Central", "20-12345678-9", "Av. Corrientes 1234", PaymentMethod.CuentaCorriente);

        Assert.Throws<InvalidOrderStateTransitionException>(() =>
            order.TransitionTo(OrderStatus.Entregado, "admin-user", "Admin"));

        Assert.Equal(OrderStatus.PendienteConfirmacion, order.EstadoActual);
        Assert.Empty(order.StatusHistories);
    }
}

