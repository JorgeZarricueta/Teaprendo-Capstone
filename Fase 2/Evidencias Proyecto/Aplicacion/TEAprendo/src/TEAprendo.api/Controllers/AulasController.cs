using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Aulas;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/aulas")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class AulasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AulasController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Comprueba si el usuario puede administrar una sede.
    private async Task<bool> PuedeAdministrarSedeAsync(Guid sedeId)
    {
        // El Administrador General puede acceder a cualquier sede.
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

        // El Director solo puede acceder a sus sedes asignadas.
        return await _context.UsuariosSedes
            .AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.SedeId == sedeId);
    }


    // Obtiene las aulas que el usuario puede administrar.
    [HttpGet]
    public async Task<ActionResult<List<AulaDto>>> ObtenerTodas()
    {
        var consulta =
            _context.Classrooms
                .AsNoTracking()
                .AsQueryable();

        // El Director solo ve aulas de sus sedes.
        if (!User.IsInRole("AdministradorGeneral"))
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario is null)
            {
                return Unauthorized();
            }

            consulta = consulta.Where(aula =>
                _context.UsuariosSedes.Any(asignacion =>
                    asignacion.UsuarioId == usuario.Id &&
                    asignacion.SedeId == aula.SiteId));
        }

        var aulas = await consulta
            .OrderBy(x => x.Name)
            .Select(x => new AulaDto
            {
                Id = x.Id,
                SedeId = x.SiteId,
                SedeNombre = x.Site.Name,
                Nombre = x.Name,
                AnioAcademico = x.AcademicYear,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(aulas);
    }


    // Obtiene un aula por ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AulaDto>> ObtenerPorId(Guid id)
    {
        var aula = await _context.Classrooms
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AulaDto
            {
                Id = x.Id,
                SedeId = x.SiteId,
                SedeNombre = x.Site.Name,
                Nombre = x.Name,
                AnioAcademico = x.AcademicYear,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (aula is null)
        {
            return NotFound(new
            {
                mensaje = "Aula no encontrada."
            });
        }

        // Comprueba acceso a la sede del aula.
        if (!await PuedeAdministrarSedeAsync(aula.SedeId))
        {
            return Forbid();
        }

        return Ok(aula);
    }


    // Obtiene las aulas de una sede autorizada.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<AulaDto>>> ObtenerPorSede(
        Guid sedeId)
    {
        var sedeExiste = await _context.Sites
            .AnyAsync(x => x.Id == sedeId);

        if (!sedeExiste)
        {
            return NotFound(new
            {
                mensaje = "Sede no encontrada."
            });
        }

        // Evita que un Director consulte otra sede.
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var aulas = await _context.Classrooms
            .AsNoTracking()
            .Where(x => x.SiteId == sedeId)
            .OrderBy(x => x.Name)
            .Select(x => new AulaDto
            {
                Id = x.Id,
                SedeId = x.SiteId,
                SedeNombre = x.Site.Name,
                Nombre = x.Name,
                AnioAcademico = x.AcademicYear,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(aulas);
    }


    // Crea un aula dentro de una sede autorizada.
    [HttpPost]
    public async Task<ActionResult<AulaDto>> Crear(
        CrearAulaRequest request)
    {
        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre del aula es obligatorio."
            });
        }

        if (request.AnioAcademico < 2000 ||
            request.AnioAcademico > 2100)
        {
            return BadRequest(new
            {
                mensaje = "El año académico no es válido."
            });
        }

        // Comprueba acceso antes de crear el aula.
        if (!await PuedeAdministrarSedeAsync(request.SedeId))
        {
            return Forbid();
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

        // Evita duplicados en la misma sede y año.
        var aulaExiste = await _context.Classrooms
            .AnyAsync(x =>
                x.SiteId == request.SedeId &&
                x.AcademicYear == request.AnioAcademico &&
                x.Name.ToLower() == nombre.ToLower());

        if (aulaExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un aula con ese nombre para ese año."
            });
        }

        var aula = new Classroom
        {
            Id = Guid.NewGuid(),
            SiteId = request.SedeId,
            Name = nombre,
            AcademicYear = request.AnioAcademico,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Classrooms.Add(aula);

        await _context.SaveChangesAsync();

        var respuesta = new AulaDto
        {
            Id = aula.Id,
            SedeId = aula.SiteId,
            SedeNombre = sede.Name,
            Nombre = aula.Name,
            AnioAcademico = aula.AcademicYear,
            Activa = aula.IsActive,
            FechaCreacionUtc = aula.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = aula.Id },
            respuesta);
    }


    // Modifica un aula autorizada.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarAulaRequest request)
    {
        var aula = await _context.Classrooms
            .FindAsync(id);

        if (aula is null)
        {
            return NotFound(new
            {
                mensaje = "Aula no encontrada."
            });
        }

        // Comprueba acceso a la sede.
        if (!await PuedeAdministrarSedeAsync(aula.SiteId))
        {
            return Forbid();
        }

        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre del aula es obligatorio."
            });
        }

        if (request.AnioAcademico < 2000 ||
            request.AnioAcademico > 2100)
        {
            return BadRequest(new
            {
                mensaje = "El año académico no es válido."
            });
        }

        // Evita duplicados en la misma sede y año.
        var aulaExiste = await _context.Classrooms
            .AnyAsync(x =>
                x.Id != id &&
                x.SiteId == aula.SiteId &&
                x.AcademicYear == request.AnioAcademico &&
                x.Name.ToLower() == nombre.ToLower());

        if (aulaExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un aula con ese nombre para ese año."
            });
        }

        aula.Name = nombre;
        aula.AcademicYear = request.AnioAcademico;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Aula actualizada correctamente."
        });
    }


    // Activa o desactiva un aula autorizada.
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoAulaRequest request)
    {
        var aula = await _context.Classrooms
            .FindAsync(id);

        if (aula is null)
        {
            return NotFound(new
            {
                mensaje = "Aula no encontrada."
            });
        }

        // Comprueba acceso a la sede.
        if (!await PuedeAdministrarSedeAsync(aula.SiteId))
        {
            return Forbid();
        }

        aula.IsActive = request.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activa
                ? "Aula activada correctamente."
                : "Aula desactivada correctamente."
        });
    }
}