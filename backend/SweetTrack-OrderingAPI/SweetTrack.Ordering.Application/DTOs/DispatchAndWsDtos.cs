using SweetTrack.Ordering.Domain.Enums;

namespace SweetTrack.Ordering.Application.DTOs;

public record StatusTransitionRequestDto(
    OrderStatus TargetStatus,
    string? Notes
);

public record AvailableTransitionDto(
    OrderStatus TargetStatus,
    string DisplayName
);

public record DispatchBoardDto(
    List<OrderResponseDto> EnPreparacion,
    List<OrderResponseDto> ListoParaDespacho,
    List<OrderResponseDto> EnReparto
);

public record WsTicketResponseDto(
    string Ticket,
    DateTime ExpiresAtUtc
);

