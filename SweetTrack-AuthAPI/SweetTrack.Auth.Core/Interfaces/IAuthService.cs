using SweetTrack.Auth.Core.DTOs;

namespace SweetTrack.Auth.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request);
        Task<bool> RegisterAsync(RegisterRequestDTO request);
    }
}