using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Usuarios;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Roles que puede gestionar un Director.
    private static readonly string[] RolesGestionables =
    {
        "Docente",
        "Terapeuta",
        "Apoderado"
    };


    public UsuariosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Comprueba si el usuario administra una sede.
    private async Task<bool> PuedeAdministrarSedeAsync(Guid sedeId)
    {
        if (User.IsInRole("AdministradorGeneral"))
        {
            return true;
        }

        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return false;
        }

        return await _context.UsuariosSedes
            .AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.SedeId == sedeId);
    }


    // Comprueba que el usuario pertenezca a la sede.
    private async Task<bool> UsuarioPerteneceSedeAsync(
        Guid usuarioId,
        Guid sedeId)
    {
        return await _context.UsuariosSedes
            .AnyAsync(x =>
                x.UsuarioId == usuarioId &&
                x.SedeId == sedeId);
    }


    // Comprueba que el rol pueda ser administrado.
    private async Task<bool> UsuarioEsGestionableAsync(
        ApplicationUser usuario)
    {
        var roles =
            await _userManager.GetRolesAsync(usuario);

        return roles.Any(rol =>
            RolesGestionables.Contains(
                rol,
                StringComparer.OrdinalIgnoreCase));
    }


    // Obtiene los usuarios de una sede.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<UsuarioSedeDto>>> ObtenerPorSede(
        Guid sedeId)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var asignaciones = await _context.UsuariosSedes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Sede)
            .Where(x => x.SedeId == sedeId)
            .ToListAsync();

        var resultado = new List<UsuarioSedeDto>();

        foreach (var asignacion in asignaciones)
        {
            var roles =
                await _userManager.GetRolesAsync(
                    asignacion.Usuario);

            resultado.Add(new UsuarioSedeDto
            {
                Id = asignacion.Usuario.Id,
                Nombre = asignacion.Usuario.Nombre,
                Apellido = asignacion.Usuario.Apellido,
                Correo =
                    asignacion.Usuario.Email ??
                    string.Empty,
                Rol =
                    roles.FirstOrDefault() ??
                    "Sin rol",
                Activo = asignacion.Usuario.Activo,
                SedeId = asignacion.Sede.Id,
                SedeNombre = asignacion.Sede.Name
            });
        }

        return Ok(resultado);
    }


    // Crea un usuario dentro de una sede.
    [HttpPost]
    public async Task<ActionResult<UsuarioSedeDto>> Crear(
        CrearUsuarioSedeRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(request.SedeId))
        {
            return Forbid();
        }

        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();
        var correo = request.Correo.Trim();
        var rolSolicitado = request.Rol.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                mensaje = "Todos los datos son obligatorios."
            });
        }

        // Busca el nombre oficial del rol.
        var rol = RolesGestionables
            .FirstOrDefault(x =>
                x.Equals(
                    rolSolicitado,
                    StringComparison.OrdinalIgnoreCase));

        if (rol is null)
        {
            return BadRequest(new
            {
                mensaje =
                    "Solo se pueden crear usuarios Docente, Terapeuta o Apoderado."
            });
        }

        var sede = await _context.Sites
            .FirstOrDefaultAsync(x =>
                x.Id == request.SedeId &&
                x.IsActive);

        if (sede is null)
        {
            return BadRequest(new
            {
                mensaje = "La sede no existe o está desactivada."
            });
        }

        var usuarioExiste =
            await _userManager.FindByEmailAsync(correo);

        if (usuarioExiste is not null)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un usuario con ese correo."
            });
        }

        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = correo,
            Email = correo,
            Nombre = nombre,
            Apellido = apellido,
            Activo = true,
            EmailConfirmed = true
        };

        // Crea el usuario con ASP.NET Identity.
        var resultadoUsuario =
            await _userManager.CreateAsync(
                usuario,
                request.Password);

        if (!resultadoUsuario.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No fue posible crear el usuario.",
                errores = resultadoUsuario.Errors
                    .Select(x => x.Description)
            });
        }

        // Asigna el rol seleccionado.
        var resultadoRol =
            await _userManager.AddToRoleAsync(
                usuario,
                rol);

        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);

            return BadRequest(new
            {
                mensaje = "No fue posible asignar el rol."
            });
        }

        // Asigna el usuario a la sede.
        _context.UsuariosSedes.Add(
            new UsuarioSede
            {
                UsuarioId = usuario.Id,
                SedeId = sede.Id
            });

        await _context.SaveChangesAsync();

        var respuesta = new UsuarioSedeDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Email ?? string.Empty,
            Rol = rol,
            Activo = usuario.Activo,
            SedeId = sede.Id,
            SedeNombre = sede.Name
        };

        return Ok(respuesta);
    }


    // Modifica los datos de un usuario de la sede.
    [HttpPut("{id:guid}/sede/{sedeId:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        Guid sedeId,
        ActualizarUsuarioRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        if (!await UsuarioPerteneceSedeAsync(id, sedeId))
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado en esta sede."
            });
        }

        var usuario =
            await _userManager.FindByIdAsync(
                id.ToString());

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        // Evita administrar Directores o Administradores.
        if (!await UsuarioEsGestionableAsync(usuario))
        {
            return Forbid();
        }

        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido))
        {
            return BadRequest(new
            {
                mensaje = "Nombre y apellido son obligatorios."
            });
        }

        usuario.Nombre = nombre;
        usuario.Apellido = apellido;

        var resultado =
            await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No fue posible actualizar el usuario."
            });
        }

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente."
        });
    }


    // Activa o desactiva un usuario.
    [HttpPatch("{id:guid}/sede/{sedeId:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        Guid sedeId,
        CambiarEstadoUsuarioRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        if (!await UsuarioPerteneceSedeAsync(id, sedeId))
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado en esta sede."
            });
        }

        var usuario =
            await _userManager.FindByIdAsync(
                id.ToString());

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        // El Director no puede desactivar otros Directores.
        if (!await UsuarioEsGestionableAsync(usuario))
        {
            return Forbid();
        }

        usuario.Activo = request.Activo;

        var resultado =
            await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No fue posible cambiar el estado."
            });
        }

        return Ok(new
        {
            mensaje = request.Activo
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente."
        });
    }
}