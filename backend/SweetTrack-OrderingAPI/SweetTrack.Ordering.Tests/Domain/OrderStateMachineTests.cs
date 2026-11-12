using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;
using SweetTrack.Ordering.Domain.StateMachine;
using Xunit;

namespace SweetTrack.Ordering.Tests.Domain;

public class OrderStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.PendienteConfirmacion, OrderStatus.EnPreparacion, "Admin", true)]
    [InlineData(OrderStatus.PendienteConfirmacion, OrderStatus.Cancelado, "Admin", true)]
    [InlineData(OrderStatus.PendienteConfirmacion, OrderStatus.Cancelado, "Client", true)]
    [InlineData(OrderStatus.PendienteConfirmacion, OrderStatus.EnPreparacion, "Client", false)]
    [InlineData(OrderStatus.EnPreparacion, OrderStatus.ListoParaDespacho, "Admin", true)]
    [InlineData(OrderStatus.EnPreparacion, OrderStatus.Cancelado, "Admin", true)]
    [InlineData(OrderStatus.EnPreparacion, OrderStatus.Cancelado, "Client", false)]
    [InlineData(OrderStatus.ListoParaDespacho, OrderStatus.EnReparto, "Admin", true)]
    [InlineData(OrderStatus.EnReparto, OrderStatus.Entregado, "Admin", true)]
    [InlineData(OrderStatus.EnReparto, OrderStatus.NoEntregado, "Admin", true)]
    [InlineData(OrderStatus.NoEntregado, OrderStatus.EnReparto, "Admin", true)]
    [InlineData(OrderStatus.NoEntregado, OrderStatus.Cancelado, "Admin", true)]
    [InlineData(OrderStatus.Entregado, OrderStatus.EnReparto, "Admin", false)]
    [InlineData(OrderStatus.Cancelado, OrderStatus.PendienteConfirmacion, "Admin", false)]
    [InlineData(OrderStatus.PendienteConfirmacion, OrderStatus.Entregado, "Admin", false)]
    public void CanTransition_EvaluatesCorrectly(OrderStatus current, OrderStatus target, string role, bool expected)
    {
        var result = OrderStateMachine.CanTransition(current, target, role);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateTransition_WhenInvalid_ThrowsInvalidOrderStateTransitionException()
    {
        var ex = Assert.Throws<InvalidOrderStateTransitionException>(() =>
            OrderStateMachine.ValidateTransition(OrderStatus.PendienteConfirmacion, OrderStatus.Entregado, "Admin"));

        Assert.Equal(OrderStatus.PendienteConfirmacion, ex.CurrentStatus);
        Assert.Equal(OrderStatus.Entregado, ex.TargetStatus);
    }

    [Fact]
    public void GetAvailableTransitions_ForClient_OnlyReturnsCanceladoWhenPendiente()
    {
        var transitionsWhenPending = OrderStateMachine.GetAvailableTransitions(OrderStatus.PendienteConfirmacion, "Client");
        Assert.Single(transitionsWhenPending);
        Assert.Contains(OrderStatus.Cancelado, transitionsWhenPending);

        var transitionsWhenInPreparation = OrderStateMachine.GetAvailableTransitions(OrderStatus.EnPreparacion, "Client");
        Assert.Empty(transitionsWhenInPreparation);
    }

    [Fact]
    public void GetAvailableTransitions_ForAdmin_ReturnsAllValidTransitions()
    {
        var transitions = OrderStateMachine.GetAvailableTransitions(OrderStatus.EnReparto, "Admin");
        Assert.Equal(2, transitions.Count);
        Assert.Contains(OrderStatus.Entregado, transitions);
        Assert.Contains(OrderStatus.NoEntregado, transitions);
    }
}
