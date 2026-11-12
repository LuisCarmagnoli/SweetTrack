using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.DTOs;

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    string KioskUserId,
    string KioskBusinessName,
    string KioskCuit,
    string DeliveryAddress,
    decimal Subtotal,
    decimal VatAmount,
    decimal TotalAmount,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    string? DeliveryNotes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<OrderItemResponseDto> Items,
    List<StatusHistoryDto> StatusHistory,
    List<OrderStatus> AvailableTransitions
);

