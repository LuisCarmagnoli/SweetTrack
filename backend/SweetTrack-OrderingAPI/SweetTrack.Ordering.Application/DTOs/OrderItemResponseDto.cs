namespace SweetTrack.Ordering.Application.DTOs;

public record OrderItemResponseDto(
    Guid Id,
    Guid ProductId,
    string Barcode,
    string ProductName,
    int Units,
    int Boxes,
    decimal UnitPrice,
    decimal LineTotal
);

