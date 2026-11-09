using Microsoft.AspNetCore.Identity;
using Moq;
using SweetTrack.Auth.Core.DTOs;
using SweetTrack.Auth.Core.Interfaces;
using SweetTrack.Auth.Services.Auth;
using Xunit;

namespace SweetTrack.Auth.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

            _authService = new AuthService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _jwtGeneratorMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnTrue_WhenUserCreationSucceeds()
        {
            // Arrange
            var request = new RegisterRequestDTO
            {
                Email = "nuevo@kiosco.com",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(m => m.RoleExistsAsync("Client"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Client"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(m => m.CreateAsync(It.Is<IdentityUser>(u => u.Email == request.Email), request.Password), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Client"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFalse_WhenUserCreationFails()
        {
            // Arrange
            var request = new RegisterRequestDTO
            {
                Email = "existente@kiosco.com",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "El usuario ya existe." }));

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Email = "inexistente@kiosco.com",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
            _userManagerMock.Verify(m => m.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Email = "kiosco@ejemplo.com",
                Password = "WrongPassword"
            };

            var user = new IdentityUser { Email = request.Email, UserName = request.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
            _jwtGeneratorMock.Verify(m => m.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Email = "kiosco@ejemplo.com",
                Password = "ValidPassword123!"
            };

            var user = new IdentityUser { Email = request.Email, UserName = request.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Client" });

            _jwtGeneratorMock
                .Setup(m => m.GenerateToken(user.Email, "Client"))
                .Returns("mocked-jwt-token-xyz");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal("mocked-jwt-token-xyz", result.Token);
            Assert.Equal("Client", result.Role);
        }
    }
}

