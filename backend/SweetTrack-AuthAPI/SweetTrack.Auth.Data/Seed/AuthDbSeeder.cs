using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SweetTrack.Auth.Data.Seed;

public static class AuthDbSeeder
{
    public static async Task SeedAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // 1. Sembrado de Roles Canónicos del Sistema
        string[] roles = ["Admin", "Client"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation("Rol '{Role}' creado exitosamente en AspNetRoles.", role);
                }
            }
        }

        // 2. Sembrado de Usuario Administrador Inicial
        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@sweettrack.com";
        var adminPassword = configuration["DefaultAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Contains("TU_PASSWORD"))
        {
            logger.LogWarning("No se configuró una contraseña válida para DefaultAdmin en IConfiguration / UserSecrets. Se omite el sembrado automático del administrador.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Usuario Administrador '{Email}' creado exitosamente y asignado al rol 'Admin'.", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Error al crear el usuario Administrador inicial: {Errors}", errors);
            }
        }
    }
}
