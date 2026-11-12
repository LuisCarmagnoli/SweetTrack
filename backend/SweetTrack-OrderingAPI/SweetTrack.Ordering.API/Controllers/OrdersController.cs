using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SweetTrack.Ordering.API.Hubs;
using SweetTrack.Ordering.Application.DTOs;
using SweetTrack.Ordering.Application.Interfaces;
using SweetTrack.Ordering.Domain.Exceptions;

namespace SweetTrack.Ordering.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderWorkflowService _workflowService;
    private readonly IHubContext<OrdersHub> _hubContext;
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderWorkflowService workflowService,
        IHubContext<OrdersHub> hubContext,
        IEmailNotificationService emailService,
        ILogger<OrdersController> logger)
    {
        _workflowService = workflowService;
        _hubContext = hubContext;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Crea y confirma un nuevo pedido mayorista para el kiosco autenticado.
    /// Valida el umbral mínimo de compra ($50.000 ARS) y emite eventos en tiempo real.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var email = GetCurrentUserEmail();

        try
        {
            var order = await _workflowService.CreateOrderAsync(userId, request, cancellationToken);

            // Notificación en tiempo real a los operadores de depósito vía SignalR
            await _hubContext.Clients.Group("warehouse-operators").SendAsync("OrderCreated", order, cancellationToken);

            // Envío asíncrono de correo de confirmación
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderConfirmationEmailAsync(
                        email,
                        order.KioskBusinessName,
                        order.OrderNumber,
                        order.TotalAmount,
                        null,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo en background para pedido {OrderNumber}", order.OrderNumber);
                }
            }, CancellationToken.None);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }
        catch (MinimumOrderAmountNotMetException ex)
        {
            return BadRequest(new { message = ex.Message, currentAmount = ex.CurrentAmount, minimumRequired = ex.MinimumRequired });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de pedidos del kiosco logueado con paginación.
    /// </summary>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var orders = await _workflowService.GetMyOrdersAsync(userId, page, pageSize, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Obtiene el detalle completo del pedido, incluyendo ítems, auditoría y transiciones disponibles.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        try
        {
            var order = await _workflowService.GetOrderByIdAsync(id, userId, role, cancellationToken);
            return Ok(order);
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ejecuta una transición de estado en el ciclo de vida del pedido validada por la máquina de estados.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(Guid id, [FromBody] StatusTransitionRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var email = GetCurrentUserEmail();

        try
        {
            var updatedOrder = await _workflowService.TransitionStatusAsync(id, request, userId, role, cancellationToken);

            // Notificación push bidireccional en tiempo real:
            // 1. Al grupo privado del kiosquero
            await _hubContext.Clients.Group($"kiosk-{updatedOrder.KioskUserId}")
                .SendAsync("OrderStatusChanged", new { orderId = id, newStatus = updatedOrder.Status, orderNumber = updatedOrder.OrderNumber }, cancellationToken);

            // 2. Al grupo de operadores de depósito
            await _hubContext.Clients.Group("warehouse-operators")
                .SendAsync("OrderStatusChanged", new { orderId = id, newStatus = updatedOrder.Status, orderNumber = updatedOrder.OrderNumber }, cancellationToken);

            // Correo de notificación de cambio de estado
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendStatusChangedEmailAsync(
                        email,
                        updatedOrder.KioskBusinessName,
                        updatedOrder.OrderNumber,
                        updatedOrder.Status.ToString(),
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de cambio de estado para pedido {OrderId}", id);
                }
            }, CancellationToken.None);

            return Ok(updatedOrder);
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { message = ex.Message, currentStatus = ex.CurrentStatus, targetStatus = ex.TargetStatus });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
               ?? User.FindFirst("email")?.Value
               ?? "cliente@sweettrack.com";
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value
               ?? (User.IsInRole("Admin") ? "Admin" : "Client");
    }
}
