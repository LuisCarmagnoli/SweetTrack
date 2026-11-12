using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Domain.Entities;

public class DeliveryRoute
{
    public Guid Id { get; set; }
    public DateTime FechaRuta { get; set; }
    public string? TransportistaId { get; set; }
    public string NombreChofer { get; set; } = string.Empty;
    public string PatenteVehiculo { get; set; } = string.Empty;
    public DeliveryRouteStatus EstadoRuta { get; set; } = DeliveryRouteStatus.Programada;
    
    // Propiedad de navegación
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public DeliveryRoute() { }

    public DeliveryRoute(
        DateTime fechaRuta,
        string nombreChofer,
        string patenteVehiculo,
        string? transportistaId = null)
    {
        FechaRuta = fechaRuta;
        NombreChofer = nombreChofer;
        PatenteVehiculo = patenteVehiculo;
        TransportistaId = transportistaId;
        EstadoRuta = DeliveryRouteStatus.Programada;
    }
}
