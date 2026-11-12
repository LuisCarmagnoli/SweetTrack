using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.DTOs;

public record StatusHistoryDto(
    Guid Id,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus,
    string ModifiedByUserId,
    string? Notes,
    DateTime TimestampUtc
);

