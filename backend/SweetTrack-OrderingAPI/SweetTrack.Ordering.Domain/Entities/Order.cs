using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.StateMachine;

namespace SweetTrack.Ordering.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeroPedido { get; set; } = string.Empty;
    
    public string KioskUserId { get; set; } = string.Empty;
    public string RazonSocialKiosco { get; set; } = string.Empty;
    public string CuitKiosco { get; set; } = string.Empty;
    public string DireccionEntrega { get; set; } = string.Empty;
    
    public decimal Subtotal { get; set; }
    public decimal MontoIva { get; set; }
    public decimal MontoTotal { get; set; }
    
    public OrderStatus EstadoActual { get; set; } = OrderStatus.PendienteConfirmacion;
    public PaymentMethod MetodoPago { get; set; } = PaymentMethod.CuentaCorriente;
    public string? NotasEntrega { get; set; }
    
    public Guid? DeliveryRouteId { get; set; }
    public DeliveryRoute? DeliveryRoute { get; set; }
    
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
    
    public byte[] RowVersion { get; set; } = [];

    // Propiedades de navegación
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public Order() { }

    public Order(
        string numeroPedido,
        string kioskUserId,
        string razonSocialKiosco,
        string cuitKiosco,
        string direccionEntrega,
        PaymentMethod metodoPago,
        string? notasEntrega = null)
    {
        NumeroPedido = numeroPedido;
        KioskUserId = kioskUserId;
        RazonSocialKiosco = razonSocialKiosco;
        CuitKiosco = cuitKiosco;
        DireccionEntrega = direccionEntrega;
        MetodoPago = metodoPago;
        NotasEntrega = notasEntrega;
        EstadoActual = OrderStatus.PendienteConfirmacion;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Agrega un ítem y recalcula los importes totales con IVA.
    /// </summary>
    public void AddItem(OrderItem item)
    {
        item.OrderId = Id;
        item.Order = this;
        Items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Recalcula Subtotal, IVA (21%) y Total en base a los ítems.
    /// </summary>
    public void RecalculateTotals()
    {
        Subtotal = Math.Round(Items.Sum(i => i.SubtotalLinea), 2);
        MontoIva = Math.Round(Subtotal * 0.21m, 2);
        MontoTotal = Math.Round(Subtotal + MontoIva, 2);
    }

    /// <summary>
    /// Ejecuta una transición de estado validada por la máquina de estados y registra el evento en el historial.
    /// </summary>
    public void TransitionTo(OrderStatus targetStatus, string modifiedByUserId, string role, string? notes = null)
    {
        OrderStateMachine.ValidateTransition(EstadoActual, targetStatus, role);

        var history = new StatusHistory(Id, EstadoActual, targetStatus, modifiedByUserId, notes)
        {
            Order = this
        };
        StatusHistories.Add(history);

        EstadoActual = targetStatus;
        FechaActualizacionUtc = DateTime.UtcNow;
    }
}
