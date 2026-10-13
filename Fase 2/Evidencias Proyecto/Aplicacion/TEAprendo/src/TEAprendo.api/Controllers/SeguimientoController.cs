using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Seguimiento;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/seguimiento")]
[Authorize(Roles = "Docente,Terapeuta")]
public class SeguimientoController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;


    public SeguimientoController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context =
            context;

        _userManager =
            userManager;
    }


    // =========================================================
    // OBSERVACIONES
    // =========================================================


    // Devuelve todas las observaciones que el profesional
    // autenticado está autorizado a consultar.
    [HttpGet("observaciones")]
    public async Task<ActionResult<List<ObservacionConductualDto>>>
        ObtenerObservaciones()
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var roles =
            await _userManager.GetRolesAsync(
                usuario);


        var alumnosAutorizados =
            await ObtenerAlumnosAutorizadosAsync(
                usuario.Id,
                roles);


        var observaciones =
            await ConstruirConsultaObservaciones()
                .Where(x =>
                    alumnosAutorizados.Contains(
                        x.Observacion.StudentId))
                .OrderByDescending(x =>
                    x.Observacion.ObservationDateTime)
                .Select(x =>
                    CrearDto(x))
                .ToListAsync();


        return Ok(
            observaciones);
    }


    // Devuelve las observaciones de un alumno específico.
    [HttpGet("alumnos/{alumnoId:guid}/observaciones")]
    public async Task<ActionResult<List<ObservacionConductualDto>>>
        ObtenerObservacionesAlumno(
            Guid alumnoId)
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var tieneAcceso =
            await PuedeAccederAlumnoAsync(
                usuario,
                alumnoId);


        if (!tieneAcceso)
        {
            return Forbid();
        }


        var observaciones =
            await ConstruirConsultaObservaciones()
                .Where(x =>
                    x.Observacion.StudentId ==
                        alumnoId)
                .OrderByDescending(x =>
                    x.Observacion.ObservationDateTime)
                .Select(x =>
                    CrearDto(x))
                .ToListAsync();


        return Ok(
            observaciones);
    }


    // Registra una observación conductual.
    [HttpPost("observaciones")]
    public async Task<ActionResult<ObservacionConductualDto>>
        CrearObservacion(
            CrearObservacionConductualRequest request)
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        if (request.AlumnoId ==
            Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un alumno."
            });
        }


        if (string.IsNullOrWhiteSpace(
            request.Contexto))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes indicar el contexto de la observación."
            });
        }


        if (string.IsNullOrWhiteSpace(
            request.Conducta))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes describir la conducta observada."
            });
        }


        var tieneAcceso =
            await PuedeAccederAlumnoAsync(
                usuario,
                request.AlumnoId);


        if (!tieneAcceso)
        {
            return Forbid();
        }


        var alumno =
            await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                        request.AlumnoId &&
                    x.IsActive);


        if (alumno is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Alumno no encontrado."
            });
        }


        var fechaObservacion =
            DateTime.SpecifyKind(
                request.FechaHoraObservacion,
                DateTimeKind.Unspecified);


        if (fechaObservacion >
            DateTime.Now)
        {
            return BadRequest(new
            {
                mensaje =
                    "La observación no puede tener una fecha futura."
            });
        }


        var observacion =
            new BehavioralObservation
            {
                Id =
                    Guid.NewGuid(),

                StudentId =
                    request.AlumnoId,

                CreatedByUserId =
                    usuario.Id,

                ObservationDateTime =
                    fechaObservacion,

                Context =
                    request.Contexto.Trim(),

                Behavior =
                    request.Conducta.Trim(),

                Antecedent =
                    LimpiarTexto(
                        request.Antecedente),

                Consequence =
                    LimpiarTexto(
                        request.Consecuencia),

                Notes =
                    LimpiarTexto(
                        request.Notas),

                CreatedAtUtc =
                    DateTime.UtcNow
            };


        _context.BehavioralObservations.Add(
            observacion);


        await _context.SaveChangesAsync();


        var respuesta =
            await ObtenerObservacionDtoAsync(
                observacion.Id);


        return Ok(
            respuesta);
    }


    // =========================================================
    // SEGURIDAD
    // =========================================================


    // Comprueba si el profesional puede acceder al alumno.
    private async Task<bool> PuedeAccederAlumnoAsync(
        ApplicationUser usuario,
        Guid alumnoId)
    {
        var roles =
            await _userManager.GetRolesAsync(
                usuario);


        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            var accesoDocente =
                await _context.Enrollments
                    .AnyAsync(x =>
                        x.StudentId ==
                            alumnoId &&
                        x.IsActive &&
                        x.Student.IsActive &&
                        x.Classroom.IsActive &&
                        _context.DocentesAulas
                            .Any(da =>
                                da.UsuarioId ==
                                    usuario.Id &&
                                da.AulaId ==
                                    x.ClassroomId));


            if (accesoDocente)
            {
                return true;
            }
        }


        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var accesoTerapeuta =
                await _context.TerapeutasAlumnos
                    .AnyAsync(x =>
                        x.UsuarioId ==
                            usuario.Id &&
                        x.AlumnoId ==
                            alumnoId &&
                        x.Alumno.IsActive);


            if (accesoTerapeuta)
            {
                return true;
            }
        }


        return false;
    }


    // Obtiene los IDs de alumnos autorizados para el profesional.
    private async Task<List<Guid>>
        ObtenerAlumnosAutorizadosAsync(
            Guid usuarioId,
            IList<string> roles)
    {
        var ids =
            new List<Guid>();


        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            var idsDocente =
                await _context.Enrollments
                    .Where(x =>
                        x.IsActive &&
                        x.Student.IsActive &&
                        x.Classroom.IsActive &&
                        _context.DocentesAulas
                            .Any(da =>
                                da.UsuarioId ==
                                    usuarioId &&
                                da.AulaId ==
                                    x.ClassroomId))
                    .Select(x =>
                        x.StudentId)
                    .Distinct()
                    .ToListAsync();


            ids.AddRange(
                idsDocente);
        }


        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var idsTerapeuta =
                await _context.TerapeutasAlumnos
                    .Where(x =>
                        x.UsuarioId ==
                            usuarioId &&
                        x.Alumno.IsActive)
                    .Select(x =>
                        x.AlumnoId)
                    .Distinct()
                    .ToListAsync();


            ids.AddRange(
                idsTerapeuta);
        }


        return ids
            .Distinct()
            .ToList();
    }


    // =========================================================
    // CONSULTAS / DTO
    // =========================================================


    private IQueryable<ObservacionConsulta>
        ConstruirConsultaObservaciones()
    {
        return
            from observacion in
                _context.BehavioralObservations
                    .AsNoTracking()

            join alumno in
                _context.Students
                    .AsNoTracking()
                on observacion.StudentId
                equals alumno.Id

            join creador in
                _context.Users
                    .AsNoTracking()
                on observacion.CreatedByUserId
                equals creador.Id

            select new ObservacionConsulta
            {
                Observacion =
                    observacion,

                Alumno =
                    alumno,

                Creador =
                    creador
            };
    }


    private static ObservacionConductualDto CrearDto(
        ObservacionConsulta x)
    {
        return new ObservacionConductualDto
        {
            Id =
                x.Observacion.Id,

            AlumnoId =
                x.Observacion.StudentId,

            AlumnoNombre =
                x.Alumno.FirstName +
                " " +
                x.Alumno.LastName,

            CreadoPorUsuarioId =
                x.Observacion.CreatedByUserId,

            CreadoPorNombre =
                x.Creador.Nombre +
                " " +
                x.Creador.Apellido,

            FechaHoraObservacion =
                x.Observacion.ObservationDateTime,

            Contexto =
                x.Observacion.Context,

            Conducta =
                x.Observacion.Behavior,

            Antecedente =
                x.Observacion.Antecedent,

            Consecuencia =
                x.Observacion.Consequence,

            Notas =
                x.Observacion.Notes,

            FechaCreacionUtc =
                x.Observacion.CreatedAtUtc
        };
    }


    private async Task<ObservacionConductualDto?>
        ObtenerObservacionDtoAsync(
            Guid id)
    {
        return await ConstruirConsultaObservaciones()
            .Where(x =>
                x.Observacion.Id ==
                    id)
            .Select(x =>
                CrearDto(x))
            .FirstOrDefaultAsync();
    }


    private static string? LimpiarTexto(
        string? texto)
    {
        return string.IsNullOrWhiteSpace(
            texto)
                ? null
                : texto.Trim();
    }


    private class ObservacionConsulta
    {
        public BehavioralObservation Observacion { get; set; }
            = null!;

        public Student Alumno { get; set; }
            = null!;

        public ApplicationUser Creador { get; set; }
            = null!;
    }
}