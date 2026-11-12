using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.DTOs;

public record OrderResponseDto(
    Guid Id,
    string OrderNumber,
    string KioskBusinessName,
    decimal Subtotal,
    decimal VatAmount,
    decimal TotalAmount,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    int TotalItems,
    DateTime CreatedAtUtc
);
