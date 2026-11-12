using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SweetTrack.Ordering.Application.DTOs;
using SweetTrack.Ordering.Application.Interfaces;
using SweetTrack.Ordering.Application.Options;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Domain.Entities;
using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;
using SweetTrack.Ordering.Domain.StateMachine;

namespace SweetTrack.Ordering.Application.Services;

public class OrderWorkflowService : IOrderWorkflowService
{
    private readonly OrderingDbContext _context;
    private readonly BusinessRulesOptions _options;
    private readonly ILogger<OrderWorkflowService> _logger;

    public OrderWorkflowService(
        OrderingDbContext context,
        IOptions<BusinessRulesOptions> options,
        ILogger<OrderWorkflowService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(
        string kioskUserId,
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("El pedido debe contener al menos un producto.", nameof(request.Items));
        }

        // 1. Calcular subtotal preliminar de las líneas
        var subtotal = request.Items.Sum(i => i.Units * i.UnitPrice);
        var iva = Math.Round(subtotal * 0.21m, 2);
        var total = Math.Round(subtotal + iva, 2);

        // 2. Validar regla de negocio de compra mínima mayorista
        if (total < _options.MinimumOrderAmount)
        {
            _logger.LogWarning("Intento de pedido por debajo del umbral mínimo: ${Total:N2} < ${_options.MinimumOrderAmount:N2}", total, _options.MinimumOrderAmount);
            throw new MinimumOrderAmountNotMetException(total, _options.MinimumOrderAmount);
        }

        // 3. Generar número correlativo de pedido con componente aleatorio para evitar colisiones concurrentes
        var randomSuffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{randomSuffix}";

        // 4. Instanciar y ensamblar la entidad Order
        var order = new Order(
            numeroPedido: orderNumber,
            kioskUserId: kioskUserId,
            razonSocialKiosco: request.RazonSocialKiosco,
            cuitKiosco: request.CuitKiosco,
            direccionEntrega: request.DireccionEntrega,
            metodoPago: request.PaymentMethod,
            notasEntrega: request.DeliveryNotes);

        foreach (var item in request.Items)
        {
            order.AddItem(new OrderItem(
                productId: item.ProductId,
                barcode: item.Barcode,
                productName: item.ProductName,
                units: item.Units,
                boxes: item.Boxes,
                unitPrice: item.UnitPrice));
        }

        // Registro de auditoría inicial
        order.StatusHistories.Add(new StatusHistory(
            orderId: order.Id,
            estadoAnterior: OrderStatus.PendienteConfirmacion,
            estadoNuevo: OrderStatus.PendienteConfirmacion,
            usuarioModificadorId: kioskUserId,
            notasMotivo: "Creación inicial del pedido por el cliente"));

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pedido {OrderNumber} creado con éxito para el usuario {UserId}. Total: ${Total:N2}", order.NumeroPedido, kioskUserId, order.MontoTotal);

        return MapToOrderResponse(order);
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(
        Guid orderId,
        string userId,
        string userRole,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories.OrderByDescending(h => h.TimestampUtc))
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        // Control de titularidad: el cliente solo puede consultar sus propios pedidos
        if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(order.KioskUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("No tienes permisos para visualizar este pedido.");
        }

        var availableTransitions = OrderStateMachine.GetAvailableTransitions(order.EstadoActual, userRole);

        return MapToOrderDetail(order, availableTransitions);
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(
        string kioskUserId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.KioskUserId == kioskUserId)
            .OrderByDescending(o => o.FechaCreacionUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToOrderResponse).ToList();
    }

    public async Task<OrderDetailDto> TransitionStatusAsync(
        Guid orderId,
        StatusTransitionRequestDto request,
        string modifiedByUserId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        // Si es cliente, validar titularidad
        if (string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(order.KioskUserId, modifiedByUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("No puedes modificar el estado de un pedido ajeno.");
        }

        // Transición de estado de dominio (valida reglas de OrderStateMachine y agrega auditoría)
        order.TransitionTo(request.TargetStatus, modifiedByUserId, role, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pedido {OrderNumber} avanzó a {NewStatus} por usuario {UserId} ({Role})", order.NumeroPedido, order.EstadoActual, modifiedByUserId, role);

        var availableTransitions = OrderStateMachine.GetAvailableTransitions(order.EstadoActual, role);
        return MapToOrderDetail(order, availableTransitions);
    }

    public async Task<DispatchBoardDto> GetDispatchBoardAsync(CancellationToken cancellationToken = default)
    {
        var activeOrders = await _context.Orders
            .Where(o => o.EstadoActual == OrderStatus.EnPreparacion ||
                        o.EstadoActual == OrderStatus.ListoParaDespacho ||
                        o.EstadoActual == OrderStatus.EnReparto)
            .Include(o => o.Items)
            .OrderBy(o => o.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        var enPreparacion = activeOrders
            .Where(o => o.EstadoActual == OrderStatus.EnPreparacion)
            .Select(MapToOrderResponse)
            .ToList();

        var listoParaDespacho = activeOrders
            .Where(o => o.EstadoActual == OrderStatus.ListoParaDespacho)
            .Select(MapToOrderResponse)
            .ToList();

        var enReparto = activeOrders
            .Where(o => o.EstadoActual == OrderStatus.EnReparto)
            .Select(MapToOrderResponse)
            .ToList();

        return new DispatchBoardDto(enPreparacion, listoParaDespacho, enReparto);
    }

    private static OrderResponseDto MapToOrderResponse(Order order)
    {
        return new OrderResponseDto(
            Id: order.Id,
            OrderNumber: order.NumeroPedido,
            KioskBusinessName: order.RazonSocialKiosco,
            Subtotal: order.Subtotal,
            VatAmount: order.MontoIva,
            TotalAmount: order.MontoTotal,
            Status: order.EstadoActual,
            PaymentMethod: order.MetodoPago,
            TotalItems: order.Items.Sum(i => i.CantidadUnidades),
            CreatedAtUtc: order.FechaCreacionUtc);
    }

    private static OrderDetailDto MapToOrderDetail(Order order, IReadOnlyList<OrderStatus> availableTransitions)
    {
        var items = order.Items.Select(i => new OrderItemResponseDto(
            Id: i.Id,
            ProductId: i.ProductId,
            Barcode: i.CodigoBarrasSnapshot,
            ProductName: i.NombreProductoSnapshot,
            Units: i.CantidadUnidades,
            Boxes: i.CantidadBultos,
            UnitPrice: i.PrecioUnitarioSnapshot,
            LineTotal: i.SubtotalLinea)).ToList();

        var history = order.StatusHistories
            .OrderByDescending(h => h.TimestampUtc)
            .Select(h => new StatusHistoryDto(
                Id: h.Id,
                PreviousStatus: h.EstadoAnterior,
                NewStatus: h.EstadoNuevo,
                ModifiedByUserId: h.UsuarioModificadorId,
                Notes: h.NotasMotivo,
                TimestampUtc: h.TimestampUtc)).ToList();

        return new OrderDetailDto(
            Id: order.Id,
            OrderNumber: order.NumeroPedido,
            KioskUserId: order.KioskUserId,
            KioskBusinessName: order.RazonSocialKiosco,
            KioskCuit: order.CuitKiosco,
            DeliveryAddress: order.DireccionEntrega,
            Subtotal: order.Subtotal,
            VatAmount: order.MontoIva,
            TotalAmount: order.MontoTotal,
            Status: order.EstadoActual,
            PaymentMethod: order.MetodoPago,
            DeliveryNotes: order.NotasEntrega,
            CreatedAtUtc: order.FechaCreacionUtc,
            UpdatedAtUtc: order.FechaActualizacionUtc,
            Items: items,
            StatusHistory: history,
            AvailableTransitions: availableTransitions.ToList());
    }
}

