using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Domain.Exceptions;

public class InvalidOrderStateTransitionException : Exception
{
    public OrderStatus CurrentStatus { get; }
    public OrderStatus TargetStatus { get; }

    public InvalidOrderStateTransitionException(OrderStatus currentStatus, OrderStatus targetStatus)
        : base($"No es posible realizar la transición de estado de '{currentStatus}' a '{targetStatus}'.")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}

