using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace SweetTrack.Ordering.API.Hubs;

public record WsTicketData(string UserId, string Role, string Email);

public class OrdersHub : Hub
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrdersHub> _logger;

    public OrdersHub(IMemoryCache cache, ILogger<OrdersHub> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var ticket = httpContext?.Request.Query["ticket"].ToString();

        string? userId = null;
        string? role = null;

        if (!string.IsNullOrEmpty(ticket) && _cache.TryGetValue(ticket, out WsTicketData? ticketData) && ticketData != null)
        {
            // Ticket efímero de un solo uso consumido
            _cache.Remove(ticket);
            userId = ticketData.UserId;
            role = ticketData.Role;
            _logger.LogInformation("Cliente conectado a SignalR mediante ticket efímero. Usuario: {UserId}, Rol: {Role}", userId, role);
        }
        else if (Context.User?.Identity?.IsAuthenticated == true)
        {
            userId = Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User.FindFirst("sub")?.Value;
            role = Context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   ?? (Context.User.IsInRole("Admin") ? "Admin" : "Client");
        }

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Conexión rechazada a OrdersHub: credencial o ticket ausente o inválido.");
            Context.Abort();
            return;
        }

        // 1. Grupo privado del kiosquero para recibir actualizaciones de sus propios pedidos
        await Groups.AddToGroupAsync(Context.ConnectionId, $"kiosk-{userId}");

        // 2. Si es Admin, agregarlo al grupo logístico de depósito
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "warehouse-operators");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Cliente {ConnectionId} desconectado de OrdersHub.", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
