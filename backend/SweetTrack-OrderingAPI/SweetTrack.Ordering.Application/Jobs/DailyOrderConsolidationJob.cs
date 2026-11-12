using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.Jobs;

[DisallowConcurrentExecution]
public class DailyOrderConsolidationJob : IJob
{
    private readonly OrderingDbContext _context;
    private readonly ILogger<DailyOrderConsolidationJob> _logger;

    public DailyOrderConsolidationJob(
        OrderingDbContext context,
        ILogger<DailyOrderConsolidationJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext jobContext)
    {
        _logger.LogInformation("[Quartz] Iniciando consolidación diaria de pedidos listos para despacho...");

        var today = DateTime.UtcNow.Date;

        // 1. Obtener pedidos listos para despacho sin ruta asignada
        var unassignedOrders = await _context.Orders
            .Where(o => o.EstadoActual == OrderStatus.ListoParaDespacho && o.DeliveryRouteId == null)
            .ToListAsync(jobContext.CancellationToken);

        if (unassignedOrders.Count == 0)
        {
            _logger.LogInformation("[Quartz] No hay pedidos pendientes de consolidación para hoy.");
            return;
        }

        // 2. Buscar o crear ruta de reparto para hoy
        var todayRoute = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.FechaRuta.Date == today && r.EstadoRuta == DeliveryRouteStatus.Programada, jobContext.CancellationToken);

        if (todayRoute == null)
        {
            todayRoute = new DeliveryRoute(
                fechaRuta: today,
                nombreChofer: "Chofer de Turno Mañana",
                patenteVehiculo: "ST-2026-LOG")
            {
                EstadoRuta = DeliveryRouteStatus.Programada
            };
            await _context.DeliveryRoutes.AddAsync(todayRoute, jobContext.CancellationToken);
            await _context.SaveChangesAsync(jobContext.CancellationToken);
        }

        // 3. Asignar los pedidos a la ruta
        foreach (var order in unassignedOrders)
        {
            order.DeliveryRouteId = todayRoute.Id;
            order.FechaActualizacionUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(jobContext.CancellationToken);

        _logger.LogInformation("[Quartz] Consolidación completada: {Count} pedidos asignados a la ruta {RouteId} ({Chofer}).", unassignedOrders.Count, todayRoute.Id, todayRoute.NombreChofer);
    }
}

