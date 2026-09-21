using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Directores;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/directores")]
[Authorize(Roles = "AdministradorGeneral")]
public class DirectoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DirectoresController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Obtiene todos los Directores.
    [HttpGet]
    public async Task<ActionResult<List<DirectorDto>>> ObtenerTodos()
    {
        var usuarios =
            await _userManager.GetUsersInRoleAsync("Director");

        var resultado = new List<DirectorDto>();

        foreach (var usuario in usuarios)
        {
            var sedes = await _context.UsuariosSedes
                .AsNoTracking()
                .Where(x => x.UsuarioId == usuario.Id)
                .Select(x => new SedeAsignadaDto
                {
                    Id = x.Sede.Id,
                    Nombre = x.Sede.Name,
                    InstitucionNombre = x.Sede.Institution.Name
                })
                .ToListAsync();

            resultado.Add(new DirectorDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Email ?? string.Empty,
                Activo = usuario.Activo,
                Sedes = sedes
            });
        }

        return Ok(resultado);
    }


    // Crea un Director y asigna sus sedes.
    [HttpPost]
    public async Task<ActionResult<DirectorDto>> Crear(
        CrearDirectorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Apellido) ||
            string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                mensaje = "Todos los datos del Director son obligatorios."
            });
        }

        var sedeIds = request.SedeIds
            .Distinct()
            .ToList();

        // El Director necesita al menos una sede.
        if (sedeIds.Count == 0)
        {
            return BadRequest(new
            {
                mensaje = "Debes asignar al menos una sede."
            });
        }

        var usuarioExiste =
            await _userManager.FindByEmailAsync(request.Correo.Trim());

        if (usuarioExiste is not null)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un usuario con ese correo."
            });
        }

        // Comprueba que todas las sedes existan y estén activas.
        var sedes = await _context.Sites
            .Where(x =>
                sedeIds.Contains(x.Id) &&
                x.IsActive)
            .ToListAsync();

        if (sedes.Count != sedeIds.Count)
        {
            return BadRequest(new
            {
                mensaje = "Una o más sedes no existen o están desactivadas."
            });
        }

        var director = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Correo.Trim(),
            Email = request.Correo.Trim(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Activo = true,
            EmailConfirmed = true
        };

        // Identity crea el usuario y protege su contraseña.
        var resultadoUsuario =
            await _userManager.CreateAsync(
                director,
                request.Password);

        if (!resultadoUsuario.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No fue posible crear el Director.",
                errores = resultadoUsuario.Errors
                    .Select(x => x.Description)
            });
        }

        // Asigna el rol Director.
        var resultadoRol =
            await _userManager.AddToRoleAsync(
                director,
                "Director");

        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(director);

            return BadRequest(new
            {
                mensaje = "No fue posible asignar el rol Director."
            });
        }

        // Asigna las sedes seleccionadas.
        foreach (var sede in sedes)
        {
            _context.UsuariosSedes.Add(new UsuarioSede
            {
                UsuarioId = director.Id,
                SedeId = sede.Id
            });
        }

        await _context.SaveChangesAsync();

        var respuesta = new DirectorDto
        {
            Id = director.Id,
            Nombre = director.Nombre,
            Apellido = director.Apellido,
            Correo = director.Email ?? string.Empty,
            Activo = director.Activo,

            Sedes = sedes.Select(x => new SedeAsignadaDto
            {
                Id = x.Id,
                Nombre = x.Name,

                InstitucionNombre =
                    _context.Institutions
                        .Where(i => i.Id == x.InstitutionId)
                        .Select(i => i.Name)
                        .First()
            }).ToList()
        };

        return Ok(respuesta);
    }


    // Cambia las sedes administradas por un Director.
    [HttpPut("{id:guid}/sedes")]
    public async Task<IActionResult> ActualizarSedes(
        Guid id,
        ActualizarSedesDirectorRequest request)
    {
        var director =
            await _userManager.FindByIdAsync(id.ToString());

        if (director is null ||
            !await _userManager.IsInRoleAsync(director, "Director"))
        {
            return NotFound(new
            {
                mensaje = "Director no encontrado."
            });
        }

        var sedeIds = request.SedeIds
            .Distinct()
            .ToList();

        if (sedeIds.Count == 0)
        {
            return BadRequest(new
            {
                mensaje = "El Director debe tener al menos una sede."
            });
        }

        var sedesValidas = await _context.Sites
            .CountAsync(x =>
                sedeIds.Contains(x.Id) &&
                x.IsActive);

        if (sedesValidas != sedeIds.Count)
        {
            return BadRequest(new
            {
                mensaje = "Una o más sedes no son válidas."
            });
        }

        var asignacionesActuales =
            await _context.UsuariosSedes
                .Where(x => x.UsuarioId == id)
                .ToListAsync();

        _context.UsuariosSedes
            .RemoveRange(asignacionesActuales);

        foreach (var sedeId in sedeIds)
        {
            _context.UsuariosSedes.Add(new UsuarioSede
            {
                UsuarioId = id,
                SedeId = sedeId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Sedes del Director actualizadas correctamente."
        });
    }
}