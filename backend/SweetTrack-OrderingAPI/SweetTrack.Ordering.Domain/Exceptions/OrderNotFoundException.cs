namespace SweetTrack.Ordering.Domain.Exceptions;

public class OrderNotFoundException : Exception
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"No se encontró la orden con identificador '{orderId}'.")
    {
        OrderId = orderId;
    }
}

