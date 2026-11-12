using SweetTrack.Ordering.Application.DTOs;

namespace SweetTrack.Ordering.Application.Interfaces;

public interface IOrderWorkflowService
{
    Task<OrderResponseDto> CreateOrderAsync(string kioskUserId, CreateOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, string userId, string userRole, CancellationToken cancellationToken = default);
    Task<List<OrderResponseDto>> GetMyOrdersAsync(string kioskUserId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> TransitionStatusAsync(Guid orderId, StatusTransitionRequestDto request, string modifiedByUserId, string role, CancellationToken cancellationToken = default);
    Task<DispatchBoardDto> GetDispatchBoardAsync(CancellationToken cancellationToken = default);
}

