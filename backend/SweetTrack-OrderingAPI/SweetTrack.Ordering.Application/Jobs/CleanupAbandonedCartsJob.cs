using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SweetTrack.Ordering.Application.Options;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.Jobs;

[DisallowConcurrentExecution]
public class CleanupAbandonedCartsJob : IJob
{
    private readonly OrderingDbContext _context;
    private readonly BusinessRulesOptions _options;
    private readonly ILogger<CleanupAbandonedCartsJob> _logger;

    public CleanupAbandonedCartsJob(
        OrderingDbContext context,
        IOptions<BusinessRulesOptions> options,
        ILogger<CleanupAbandonedCartsJob> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext jobContext)
    {
        var thresholdDate = DateTime.UtcNow.AddHours(-_options.AbandonedCartExpirationHours);

        var expiredOrders = await _context.Orders
            .Where(o => o.EstadoActual == OrderStatus.PendienteConfirmacion && o.FechaCreacionUtc < thresholdDate)
            .Include(o => o.StatusHistories)
            .ToListAsync(jobContext.CancellationToken);

        if (expiredOrders.Count == 0)
        {
            return;
        }

        _logger.LogInformation("[Quartz] Cancelando {Count} pedidos pendientes expirados (antigüedad > {Hours}h)...", expiredOrders.Count, _options.AbandonedCartExpirationHours);

        foreach (var order in expiredOrders)
        {
            order.StatusHistories.Add(new StatusHistory(
                orderId: order.Id,
                estadoAnterior: order.EstadoActual,
                estadoNuevo: OrderStatus.Cancelado,
                usuarioModificadorId: "system-quartz-job",
                notasMotivo: $"Cancelación automática: superó la ventana de {_options.AbandonedCartExpirationHours} horas sin confirmación"));

            order.EstadoActual = OrderStatus.Cancelado;
            order.FechaActualizacionUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(jobContext.CancellationToken);
        _logger.LogInformation("[Quartz] Cancelación automática de pedidos expirados completada.");
    }
}
