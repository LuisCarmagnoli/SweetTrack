using Microsoft.AspNetCore.Mvc;
using SweetTrack.Auth.Core.DTOs;
using SweetTrack.Auth.Core.Interfaces;

namespace SweetTrack.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var isSuccessful = await _authService.RegisterAsync(request);

            if (!isSuccessful)
            {
                return BadRequest(new { Message = "Error al registrar el usuario. El correo podría ya estar en uso." });
            }

            return Ok(new { Message = "Usuario registrado con éxito." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { Message = "Credenciales incorrectas." });
            }

            return Ok(response);
        }
    }
}