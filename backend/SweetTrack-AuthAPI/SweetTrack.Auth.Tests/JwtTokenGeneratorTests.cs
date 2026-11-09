using Microsoft.Extensions.Options;
using SweetTrack.Auth.Services.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace SweetTrack.Auth.Tests
{
    public class JwtTokenGeneratorTests
    {
        private readonly JwtOptions _options = new()
        {
            Issuer = "SweetTrackAuth",
            Audience = "SweetTrackClients",
            Secret = "SuperSecretKeyForSweetTrackJwtTokenGeneration1234567890!"
        };

        [Fact]
        public void GenerateToken_ShouldReturnValidJwt_WithExpectedClaims()
        {
            // Arrange
            var optionsMock = Options.Create(_options);
            var generator = new JwtTokenGenerator(optionsMock);
            var email = "kiosco@ejemplo.com";
            var role = "Client";

            // Act
            var tokenString = generator.GenerateToken(email, role);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(tokenString));

            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(tokenString));

            var token = handler.ReadJwtToken(tokenString);
            Assert.Equal(_options.Issuer, token.Issuer);
            Assert.Contains(_options.Audience, token.Audiences);

            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
            Assert.NotNull(emailClaim);
            Assert.Equal(email, emailClaim.Value);

            var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
            Assert.NotNull(roleClaim);
            Assert.Equal(role, roleClaim.Value);
        }
    }
}

