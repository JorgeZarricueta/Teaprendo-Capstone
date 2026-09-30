using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Terapeutas;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/terapeutas")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class TerapeutasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TerapeutasController(
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


    // Obtiene terapeutas de una sede y sus alumnos.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<TerapeutaDto>>> ObtenerPorSede(
        Guid sedeId)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var usuariosSede =
            await _context.UsuariosSedes
                .Where(x => x.SedeId == sedeId)
                .Select(x => x.UsuarioId)
                .ToListAsync();

        var terapeutasRol =
            await _userManager.GetUsersInRoleAsync(
                "Terapeuta");

        var terapeutas =
            terapeutasRol
                .Where(x =>
                    usuariosSede.Contains(x.Id))
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Apellido)
                .ToList();

        var resultado =
            new List<TerapeutaDto>();

        foreach (var terapeuta in terapeutas)
        {
            var alumnos =
                await _context.TerapeutasAlumnos
                    .AsNoTracking()
                    .Where(x =>
                        x.UsuarioId == terapeuta.Id)
                    .Join(
                        _context.Enrollments
                            .Where(x =>
                                x.IsActive &&
                                x.Classroom.SiteId == sedeId),
                        relacion => relacion.AlumnoId,
                        matricula => matricula.StudentId,
                        (relacion, matricula) =>
                            new AlumnoAsignadoTerapeutaDto
                            {
                                Id =
                                    matricula.Student.Id,

                                Nombre =
                                    matricula.Student.FirstName,

                                Apellido =
                                    matricula.Student.LastName,

                                AulaNombre =
                                    matricula.Classroom.Name,

                                Activo =
                                    matricula.Student.IsActive
                            })
                    .OrderBy(x => x.Nombre)
                    .ThenBy(x => x.Apellido)
                    .ToListAsync();

            resultado.Add(
                new TerapeutaDto
                {
                    Id = terapeuta.Id,
                    Nombre = terapeuta.Nombre,
                    Apellido = terapeuta.Apellido,
                    Correo =
                        terapeuta.Email ??
                        string.Empty,
                    Activo = terapeuta.Activo,
                    Alumnos = alumnos
                });
        }

        return Ok(resultado);
    }


    // Asigna alumnos al terapeuta.
    [HttpPut("{terapeutaId:guid}/sede/{sedeId:guid}/alumnos")]
    public async Task<IActionResult> AsignarAlumnos(
        Guid terapeutaId,
        Guid sedeId,
        AsignarAlumnosTerapeutaRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var terapeuta =
            await _userManager.FindByIdAsync(
                terapeutaId.ToString());

        if (terapeuta is null)
        {
            return NotFound(new
            {
                mensaje = "Terapeuta no encontrado."
            });
        }

        // Comprueba que el terapeuta pertenezca a la sede.
        var perteneceSede =
            await _context.UsuariosSedes
                .AnyAsync(x =>
                    x.UsuarioId == terapeutaId &&
                    x.SedeId == sedeId);

        if (!perteneceSede)
        {
            return BadRequest(new
            {
                mensaje =
                    "El terapeuta no pertenece a esta sede."
            });
        }

        // Comprueba el rol.
        var roles =
            await _userManager.GetRolesAsync(
                terapeuta);

        if (!roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje =
                    "El usuario seleccionado no es un Terapeuta."
            });
        }

        var alumnoIds =
            request.AlumnoIds
                .Distinct()
                .ToList();

        // Valida que todos los alumnos pertenezcan a la sede.
        var alumnosValidos =
            await _context.Enrollments
                .Where(x =>
                    x.IsActive &&
                    x.Classroom.SiteId == sedeId &&
                    x.Student.IsActive &&
                    alumnoIds.Contains(x.StudentId))
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

        if (alumnosValidos.Count != alumnoIds.Count)
        {
            return BadRequest(new
            {
                mensaje =
                    "Uno o más alumnos no pertenecen actualmente a esta sede."
            });
        }

        var alumnosSede =
            await _context.Enrollments
                .Where(x =>
                    x.IsActive &&
                    x.Classroom.SiteId == sedeId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

        var relacionesActuales =
            await _context.TerapeutasAlumnos
                .Where(x =>
                    x.UsuarioId == terapeutaId &&
                    alumnosSede.Contains(x.AlumnoId))
                .ToListAsync();

        // Reemplaza las asignaciones actuales.
        _context.TerapeutasAlumnos.RemoveRange(
            relacionesActuales);

        foreach (var alumnoId in alumnoIds)
        {
            _context.TerapeutasAlumnos.Add(
                new TerapeutaAlumno
                {
                    UsuarioId = terapeutaId,
                    AlumnoId = alumnoId
                });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje =
                "Alumnos del terapeuta actualizados correctamente."
        });
    }
}