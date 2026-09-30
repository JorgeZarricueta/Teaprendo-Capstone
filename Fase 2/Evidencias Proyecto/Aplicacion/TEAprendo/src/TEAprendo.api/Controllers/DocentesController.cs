using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Docentes;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/docentes")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class DocentesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DocentesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Comprueba si el usuario administra una sede.
    private async Task<bool> PuedeAdministrarSedeAsync(
        Guid sedeId)
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


    // Obtiene los docentes de una sede y sus aulas.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<DocenteDto>>> ObtenerPorSede(
        Guid sedeId)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        // Usuarios asociados a la sede.
        var usuariosSede =
            await _context.UsuariosSedes
                .Where(x => x.SedeId == sedeId)
                .Select(x => x.UsuarioId)
                .ToListAsync();

        // Usuarios que poseen el rol Docente.
        var docentesRol =
            await _userManager.GetUsersInRoleAsync(
                "Docente");

        var docentes =
            docentesRol
                .Where(x =>
                    usuariosSede.Contains(x.Id))
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Apellido)
                .ToList();

        var resultado =
            new List<DocenteDto>();

        foreach (var docente in docentes)
        {
            var aulas =
                await _context.DocentesAulas
                    .AsNoTracking()
                    .Include(x => x.Aula)
                    .Where(x =>
                        x.UsuarioId == docente.Id &&
                        x.Aula.SiteId == sedeId)
                    .OrderBy(x => x.Aula.Name)
                    .Select(x =>
                        new AulaAsignadaDocenteDto
                        {
                            Id = x.Aula.Id,
                            Nombre = x.Aula.Name,
                            AnioAcademico =
                                x.Aula.AcademicYear
                        })
                    .ToListAsync();

            resultado.Add(
                new DocenteDto
                {
                    Id = docente.Id,
                    Nombre = docente.Nombre,
                    Apellido = docente.Apellido,
                    Correo =
                        docente.Email ??
                        string.Empty,
                    Activo = docente.Activo,
                    Aulas = aulas
                });
        }

        return Ok(resultado);
    }


    // Asigna las aulas que tendrá a cargo el docente.
    [HttpPut("{docenteId:guid}/sede/{sedeId:guid}/aulas")]
    public async Task<IActionResult> AsignarAulas(
        Guid docenteId,
        Guid sedeId,
        AsignarAulasDocenteRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var docente =
            await _userManager.FindByIdAsync(
                docenteId.ToString());

        if (docente is null)
        {
            return NotFound(new
            {
                mensaje = "Docente no encontrado."
            });
        }

        // Comprueba que el usuario pertenezca a la sede.
        var perteneceSede =
            await _context.UsuariosSedes
                .AnyAsync(x =>
                    x.UsuarioId == docenteId &&
                    x.SedeId == sedeId);

        if (!perteneceSede)
        {
            return BadRequest(new
            {
                mensaje =
                    "El docente no pertenece a esta sede."
            });
        }

        // Comprueba que realmente tenga el rol Docente.
        var roles =
            await _userManager.GetRolesAsync(docente);

        if (!roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje =
                    "El usuario seleccionado no es un Docente."
            });
        }

        // Elimina IDs repetidos.
        var aulaIds =
            request.AulaIds
                .Distinct()
                .ToList();

        // Comprueba que todas las aulas sean de la sede.
        var aulasValidas =
            await _context.Classrooms
                .Where(x =>
                    aulaIds.Contains(x.Id) &&
                    x.SiteId == sedeId &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

        if (aulasValidas.Count != aulaIds.Count)
        {
            return BadRequest(new
            {
                mensaje =
                    "Una o más aulas no pertenecen a la sede o están desactivadas."
            });
        }

        // Obtiene las asignaciones actuales de esta sede.
        var asignacionesActuales =
            await _context.DocentesAulas
                .Include(x => x.Aula)
                .Where(x =>
                    x.UsuarioId == docenteId &&
                    x.Aula.SiteId == sedeId)
                .ToListAsync();

        // Reemplaza las asignaciones de esta sede.
        _context.DocentesAulas.RemoveRange(
            asignacionesActuales);

        foreach (var aulaId in aulaIds)
        {
            _context.DocentesAulas.Add(
                new DocenteAula
                {
                    UsuarioId = docenteId,
                    AulaId = aulaId
                });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje =
                "Aulas del docente actualizadas correctamente."
        });
    }
}