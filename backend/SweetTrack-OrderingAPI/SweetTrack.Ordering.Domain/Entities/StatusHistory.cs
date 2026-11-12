using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Domain.Entities;

public class StatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    
    public OrderStatus EstadoAnterior { get; set; }
    public OrderStatus EstadoNuevo { get; set; }
    
    public string UsuarioModificadorId { get; set; } = string.Empty;
    public string? NotasMotivo { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    // Propiedad de navegación
    public Order Order { get; set; } = null!;

    public StatusHistory() { }

    public StatusHistory(
        Guid orderId,
        OrderStatus estadoAnterior,
        OrderStatus estadoNuevo,
        string usuarioModificadorId,
        string? notasMotivo = null)
    {
        OrderId = orderId;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        UsuarioModificadorId = usuarioModificadorId;
        NotasMotivo = notasMotivo;
        TimestampUtc = DateTime.UtcNow;
    }
}
