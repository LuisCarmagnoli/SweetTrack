namespace SweetTrack.Auth.Core.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string email, string role);
    }
}