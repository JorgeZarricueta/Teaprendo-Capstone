using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace TEAprendo.Infrastructure.Identity;

public static class IdentitySeeder
{
    // Crea los roles principales si no existen.
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        {
            "AdministradorGeneral",
            "Director",
            "Docente",
            "Terapeuta",
            "Apoderado"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };

                await roleManager.CreateAsync(role);
            }
        }
    }


    // Crea o actualiza el administrador general inicial.
    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var correo =
            configuration["AdministradorInicial:Correo"];

        var password =
            configuration["AdministradorInicial:Password"];

        var nombre =
            configuration["AdministradorInicial:Nombre"];

        var apellido =
            configuration["AdministradorInicial:Apellido"];

        // Verifica que exista la configuración básica.
        if (string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var usuario =
            await userManager.FindByEmailAsync(correo);

        // Si ya existe, asegura que tenga el nuevo rol.
        if (usuario is not null)
        {
            if (!await userManager.IsInRoleAsync(
                usuario,
                "AdministradorGeneral"))
            {
                await userManager.AddToRoleAsync(
                    usuario,
                    "AdministradorGeneral");
            }

            return;
        }

        // Crea el administrador inicial.
        var nuevoUsuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = correo,
            Email = correo,
            Nombre = nombre ?? "Administrador",
            Apellido = apellido ?? "TEAprendo",
            Activo = true,
            EmailConfirmed = true
        };

        var resultado =
            await userManager.CreateAsync(
                nuevoUsuario,
                password);

        // Asigna el rol global.
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(
                nuevoUsuario,
                "AdministradorGeneral");
        }
    }
}