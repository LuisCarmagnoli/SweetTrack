using Microsoft.EntityFrameworkCore;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Data.Seed;

public static class OrderingDbSeeder
{
    public static async Task SeedAsync(OrderingDbContext context)
    {
        if (await context.DeliveryRoutes.AnyAsync())
        {
            return;
        }

        var hoy = DateTime.UtcNow.Date;

        var routes = new List<DeliveryRoute>
        {
            new DeliveryRoute(
                fechaRuta: hoy,
                nombreChofer: "Carlos Rodríguez",
                patenteVehiculo: "AF 123 CD",
                transportistaId: "trans-001")
            {
                EstadoRuta = DeliveryRouteStatus.Programada
            },
            new DeliveryRoute(
                fechaRuta: hoy,
                nombreChofer: "Mariano Gómez",
                patenteVehiculo: "AE 456 GH",
                transportistaId: "trans-002")
            {
                EstadoRuta = DeliveryRouteStatus.Programada
            },
            new DeliveryRoute(
                fechaRuta: hoy.AddDays(1),
                nombreChofer: "Luciano Benítez",
                patenteVehiculo: "AG 789 JK",
                transportistaId: "trans-003")
            {
                EstadoRuta = DeliveryRouteStatus.Programada
            }
        };

        await context.DeliveryRoutes.AddRangeAsync(routes);
        await context.SaveChangesAsync();
    }
}
