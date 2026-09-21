using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TEAprendo.Contracts.Autenticacion;
using TEAprendo.Infrastructure.Identity;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/autenticacion")]
public class AutenticacionController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AutenticacionController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }


    // Inicia sesión utilizando correo y contraseña.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Busca al usuario por correo.
        var usuario =
            await _userManager.FindByEmailAsync(request.Correo);

        if (usuario is null)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        // Evita el acceso de usuarios desactivados.
        if (!usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "La cuenta se encuentra desactivada."
            });
        }

        // Comprueba la contraseña e inicia la sesión.
        var resultado =
            await _signInManager.PasswordSignInAsync(
                usuario,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: false);

        if (!resultado.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        // Obtiene los roles del usuario.
        var roles =
            await _userManager.GetRolesAsync(usuario);

        return Ok(new
        {
            mensaje = "Inicio de sesión correcto.",
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email,
                roles
            }
        });
    }


    // Devuelve los datos del usuario que tiene la sesión iniciada.
    [Authorize]
    [HttpGet("usuario-actual")]
    public async Task<ActionResult<UsuarioActualDto>> UsuarioActual()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var roles =
            await _userManager.GetRolesAsync(usuario);

        var respuesta = new UsuarioActualDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Email ?? string.Empty,
            Roles = roles.ToList()
        };

        return Ok(respuesta);
    }


    // Cierra la sesión actual.
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new
        {
            mensaje = "Sesión cerrada correctamente."
        });
    }
}