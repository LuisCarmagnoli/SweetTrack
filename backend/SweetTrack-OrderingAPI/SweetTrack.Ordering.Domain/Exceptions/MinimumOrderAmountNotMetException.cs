namespace SweetTrack.Ordering.Domain.Exceptions;

public class MinimumOrderAmountNotMetException : Exception
{
    public decimal CurrentAmount { get; }
    public decimal MinimumRequired { get; }

    public MinimumOrderAmountNotMetException(decimal currentAmount, decimal minimumRequired)
        : base($"El monto total del pedido (${currentAmount:N2}) no alcanza el umbral mínimo mayorista requerido (${minimumRequired:N2}).")
    {
        CurrentAmount = currentAmount;
        MinimumRequired = minimumRequired;
    }
}

