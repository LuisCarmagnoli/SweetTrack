using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SweetTrack.Ordering.API.Hubs;
using SweetTrack.Ordering.Application.DTOs;

namespace SweetTrack.Ordering.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class WsTicketController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WsTicketController> _logger;

    public WsTicketController(IMemoryCache cache, ILogger<WsTicketController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Emite un ticket criptográfico efímero de corta duración (30 segundos) para autorizar la conexión al WebSocket de SignalR sin exponer JWTs en la URL.
    /// </summary>
    [HttpPost("ws-ticket")]
    [ProducesResponseType(typeof(WsTicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GenerateWsTicket()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("email")?.Value
                     ?? User.Identity?.Name;

        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value ?? "desconocido@sweettrack.com";

        var role = User.FindFirst(ClaimTypes.Role)?.Value
                   ?? (User.IsInRole("Admin") ? "Admin" : "Client");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario desde el token." });
        }

        var ticket = Guid.NewGuid().ToString("N");
        var expiresAtUtc = DateTime.UtcNow.AddSeconds(30);

        _cache.Set(ticket, new WsTicketData(userId, role, email), TimeSpan.FromSeconds(30));

        _logger.LogDebug("Ticket efímero generado para usuario {UserId} ({Role}) válido hasta {ExpiresAt}", userId, role, expiresAtUtc);

        return Ok(new WsTicketResponseDto(ticket, expiresAtUtc));
    }
}

