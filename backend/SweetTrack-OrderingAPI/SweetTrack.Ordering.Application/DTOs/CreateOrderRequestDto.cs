using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.DTOs;

public record CreateOrderRequestDto(
    string RazonSocialKiosco,
    string CuitKiosco,
    string DireccionEntrega,
    PaymentMethod PaymentMethod,
    List<CreateOrderItemDto> Items,
    string? DeliveryNotes
);
