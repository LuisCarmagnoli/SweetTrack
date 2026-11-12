namespace SweetTrack.Ordering.Application.DTOs;

public record CreateOrderItemDto(
    Guid ProductId,
    string Barcode,
    string ProductName,
    int Units,
    int Boxes,
    decimal UnitPrice
);

