using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Apoderados;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/apoderados")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class ApoderadosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApoderadosController(
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


    // Obtiene los apoderados de una sede.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<ApoderadoDto>>> ObtenerPorSede(
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

        var usuariosApoderados =
            await _userManager.GetUsersInRoleAsync(
                "Apoderado");

        var apoderados =
            usuariosApoderados
                .Where(x =>
                    usuariosSede.Contains(x.Id))
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Apellido)
                .ToList();

        var resultado =
            new List<ApoderadoDto>();

        foreach (var apoderado in apoderados)
        {
            var alumnos =
                await _context.ApoderadosAlumnos
                    .AsNoTracking()
                    .Where(x =>
                        x.UsuarioId == apoderado.Id)
                    .Join(
                        _context.Enrollments
                            .Where(x =>
                                x.IsActive &&
                                x.Classroom.SiteId == sedeId),
                        relacion => relacion.AlumnoId,
                        matricula => matricula.StudentId,
                        (relacion, matricula) =>
                            new AlumnoAsignadoApoderadoDto
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
                new ApoderadoDto
                {
                    Id = apoderado.Id,
                    Nombre = apoderado.Nombre,
                    Apellido = apoderado.Apellido,
                    Correo =
                        apoderado.Email ??
                        string.Empty,
                    Activo = apoderado.Activo,
                    Alumnos = alumnos
                });
        }

        return Ok(resultado);
    }


    // Asigna alumnos al apoderado.
    [HttpPut("{apoderadoId:guid}/sede/{sedeId:guid}/alumnos")]
    public async Task<IActionResult> AsignarAlumnos(
        Guid apoderadoId,
        Guid sedeId,
        AsignarAlumnosApoderadoRequest request)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var apoderado =
            await _userManager.FindByIdAsync(
                apoderadoId.ToString());

        if (apoderado is null)
        {
            return NotFound(new
            {
                mensaje = "Apoderado no encontrado."
            });
        }

        // Comprueba que pertenezca a la sede.
        var perteneceSede =
            await _context.UsuariosSedes
                .AnyAsync(x =>
                    x.UsuarioId == apoderadoId &&
                    x.SedeId == sedeId);

        if (!perteneceSede)
        {
            return BadRequest(new
            {
                mensaje =
                    "El apoderado no pertenece a esta sede."
            });
        }

        // Comprueba que tenga el rol Apoderado.
        var roles =
            await _userManager.GetRolesAsync(
                apoderado);

        if (!roles.Contains(
            "Apoderado",
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje =
                    "El usuario seleccionado no es un Apoderado."
            });
        }

        var alumnoIds =
            request.AlumnoIds
                .Distinct()
                .ToList();

        // Comprueba que los alumnos estén matriculados
        // actualmente en esta sede.
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

        // Obtiene las relaciones actuales correspondientes
        // a alumnos matriculados en esta sede.
        var alumnosSede =
            await _context.Enrollments
                .Where(x =>
                    x.IsActive &&
                    x.Classroom.SiteId == sedeId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

        var relacionesActuales =
            await _context.ApoderadosAlumnos
                .Where(x =>
                    x.UsuarioId == apoderadoId &&
                    alumnosSede.Contains(x.AlumnoId))
                .ToListAsync();

        // Reemplaza las asociaciones de esta sede.
        _context.ApoderadosAlumnos.RemoveRange(
            relacionesActuales);

        foreach (var alumnoId in alumnoIds)
        {
            _context.ApoderadosAlumnos.Add(
                new ApoderadoAlumno
                {
                    UsuarioId = apoderadoId,
                    AlumnoId = alumnoId
                });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje =
                "Alumnos del apoderado actualizados correctamente."
        });
    }
}