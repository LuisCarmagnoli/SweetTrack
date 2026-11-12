using SweetTrack.Ordering.Domain.Enums;
using SweetTrack.Ordering.Domain.Exceptions;

namespace SweetTrack.Ordering.Domain.StateMachine;

public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.PendienteConfirmacion] = [OrderStatus.EnPreparacion, OrderStatus.Cancelado],
        [OrderStatus.EnPreparacion] = [OrderStatus.ListoParaDespacho, OrderStatus.Cancelado],
        [OrderStatus.ListoParaDespacho] = [OrderStatus.EnReparto],
        [OrderStatus.EnReparto] = [OrderStatus.Entregado, OrderStatus.NoEntregado],
        [OrderStatus.NoEntregado] = [OrderStatus.EnReparto, OrderStatus.Cancelado],
        [OrderStatus.Entregado] = [],
        [OrderStatus.Cancelado] = []
    };

    /// <summary>
    /// Verifica si la transición de estado es válida según las reglas del ciclo de vida y los permisos del rol.
    /// </summary>
    public static bool CanTransition(OrderStatus current, OrderStatus target, string role)
    {
        if (!AllowedTransitions.TryGetValue(current, out var targets) || !targets.Contains(target))
        {
            return false;
        }

        // Si es rol de Kiosquero/Cliente, solo puede cancelar cuando aún está en PendienteConfirmacion
        if (string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
        {
            return current == OrderStatus.PendienteConfirmacion && target == OrderStatus.Cancelado;
        }

        // El rol Admin tiene acceso a todas las transiciones canónicas permitidas
        return true;
    }

    /// <summary>
    /// Valida la transición lanzando una excepción de dominio si es inválida.
    /// </summary>
    public static void ValidateTransition(OrderStatus current, OrderStatus target, string role)
    {
        if (!CanTransition(current, target, role))
        {
            throw new InvalidOrderStateTransitionException(current, target);
        }
    }

    /// <summary>
    /// Obtiene las transiciones disponibles según el estado actual y el rol del usuario conectado.
    /// </summary>
    public static IReadOnlyList<OrderStatus> GetAvailableTransitions(OrderStatus current, string role)
    {
        if (!AllowedTransitions.TryGetValue(current, out var targets))
        {
            return Array.Empty<OrderStatus>();
        }

        if (string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
        {
            return current == OrderStatus.PendienteConfirmacion
                ? new[] { OrderStatus.Cancelado }
                : Array.Empty<OrderStatus>();
        }

        return targets.AsReadOnly();
    }
}
