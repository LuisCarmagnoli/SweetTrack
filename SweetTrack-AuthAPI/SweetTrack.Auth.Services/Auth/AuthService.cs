using Microsoft.AspNetCore.Identity;
using SweetTrack.Auth.Core.DTOs;
using SweetTrack.Auth.Core.Interfaces;

namespace SweetTrack.Auth.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDTO request)
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Verifica si el rol existe, si no, lo crea (útil para la primera ejecución)
                if (!await _roleManager.RoleExistsAsync("Client"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Client"));
                }

                await _userManager.AddToRoleAsync(user, "Client");
                return true;
            }

            return false;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            bool isValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (user == null || isValid == false)
            {
                return null; // Credenciales inválidas
            }

            // Obtener roles del usuario (tomamos el primero para el token)
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Client";

            // Generar Token
            var token = _jwtTokenGenerator.GenerateToken(user.Email, role);

            return new AuthResponseDTO
            {
                Email = user.Email,
                Token = token,
                Role = role
            };
        }
    }
}