using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Crisis;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;


[ApiController]
[Route("api/eventos-crisis")]
[Authorize(Roles = "Docente,Terapeuta")]
public class EventosCrisisController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;


    public EventosCrisisController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context =
            context;

        _userManager =
            userManager;
    }


    // =========================================================
    // CONSULTA
    // =========================================================


    // Devuelve el historial de crisis de un alumno autorizado.
    [HttpGet("alumno/{alumnoId:guid}")]
    public async Task<ActionResult<List<EventoCrisisDto>>>
        ObtenerEventosAlumno(
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


        var eventos =
            await ConstruirConsultaEventos()
                .Where(x =>
                    x.Evento.StudentId ==
                        alumnoId)
                .OrderByDescending(x =>
                    x.Evento.EventDateTime)
                .Select(x =>
                    CrearDto(x))
                .ToListAsync();


        return Ok(
            eventos);
    }


    // =========================================================
    // REGISTRO
    // =========================================================


    // Registra una situación de crisis
    // desde el perfil del estudiante.
    [HttpPost]
    public async Task<ActionResult<EventoCrisisDto>>
        CrearEvento(
            CrearEventoCrisisRequest request)
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
                    "Debes indicar el contexto del evento."
            });
        }


        if (request.Contexto.Trim().Length >
            100)
        {
            return BadRequest(new
            {
                mensaje =
                    "El contexto admite hasta 100 caracteres."
            });
        }


        if (string.IsNullOrWhiteSpace(
            request.DescripcionEvento))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes describir brevemente lo ocurrido."
            });
        }


        if (request.DescripcionEvento
            .Trim()
            .Length > 1500)
        {
            return BadRequest(new
            {
                mensaje =
                    "La descripción admite hasta 1500 caracteres."
            });
        }


        if (request.DuracionMinutos.HasValue &&
            (request.DuracionMinutos.Value < 1 ||
             request.DuracionMinutos.Value > 1440))
        {
            return BadRequest(new
            {
                mensaje =
                    "La duración debe estar entre 1 y 1440 minutos."
            });
        }


        if (!TextoValido(
                request.Desencadenante,
                1000))
        {
            return BadRequest(new
            {
                mensaje =
                    "El desencadenante admite hasta 1000 caracteres."
            });
        }


        if (!TextoValido(
                request.ApoyoInmediato,
                1500))
        {
            return BadRequest(new
            {
                mensaje =
                    "El apoyo inmediato admite hasta 1500 caracteres."
            });
        }


        if (!TextoValido(
                request.Resultado,
                1500))
        {
            return BadRequest(new
            {
                mensaje =
                    "El resultado admite hasta 1500 caracteres."
            });
        }


        if (!TextoValido(
                request.Notas,
                1000))
        {
            return BadRequest(new
            {
                mensaje =
                    "Las notas admiten hasta 1000 caracteres."
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


        var alumnoExiste =
            await _context.Students
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id ==
                        request.AlumnoId &&
                    x.IsActive);


        if (!alumnoExiste)
        {
            return NotFound(new
            {
                mensaje =
                    "Alumno no encontrado."
            });
        }


        var fechaEvento =
            DateTime.SpecifyKind(
                request.FechaHoraEvento,
                DateTimeKind.Unspecified);


        if (fechaEvento == default)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes indicar la fecha y hora del evento."
            });
        }


        // Se permite un pequeño margen
        // por diferencias de reloj del dispositivo.
        if (fechaEvento >
            DateTime.Now.AddMinutes(5))
        {
            return BadRequest(new
            {
                mensaje =
                    "El evento no puede tener una fecha futura."
            });
        }


        var evento =
            new CrisisEvent
            {
                Id =
                    Guid.NewGuid(),

                StudentId =
                    request.AlumnoId,

                CreatedByUserId =
                    usuario.Id,

                EventDateTime =
                    fechaEvento,

                DurationMinutes =
                    request.DuracionMinutos,

                Context =
                    request.Contexto.Trim(),

                EventDescription =
                    request.DescripcionEvento
                        .Trim(),

                Trigger =
                    LimpiarTexto(
                        request.Desencadenante),

                ImmediateSupport =
                    LimpiarTexto(
                        request.ApoyoInmediato),

                Outcome =
                    LimpiarTexto(
                        request.Resultado),

                WasStabilized =
                    request.SeEstabilizo,

                RequiredAdditionalSupport =
                    request.RequirioApoyoAdicional,

                Notes =
                    LimpiarTexto(
                        request.Notas),

                CreatedAtUtc =
                    DateTime.UtcNow
            };


        _context.CrisisEvents.Add(
            evento);


        await _context.SaveChangesAsync();


        var respuesta =
            await ObtenerEventoDtoAsync(
                evento.Id);


        if (respuesta is null)
        {
            return StatusCode(
                500,
                new
                {
                    mensaje =
                        "El evento fue registrado, pero no fue posible recuperar su información."
                });
        }


        return Ok(
            respuesta);
    }


    // =========================================================
    // SEGURIDAD
    // =========================================================


    // Comprueba que el profesional tenga
    // acceso al alumno consultado.
    private async Task<bool>
        PuedeAccederAlumnoAsync(
            ApplicationUser usuario,
            Guid alumnoId)
    {
        var roles =
            await _userManager.GetRolesAsync(
                usuario);


        // Docente:
        // acceso mediante el aula asignada.
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


        // Terapeuta:
        // acceso mediante asignación directa.
        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var accesoTerapeuta =
                await _context.Enrollments
                    .AnyAsync(x =>
                        x.StudentId ==
                            alumnoId &&
                        x.IsActive &&
                        x.Student.IsActive &&
                        x.Classroom.IsActive &&
                        _context.TerapeutasAlumnos
                            .Any(ta =>
                                ta.UsuarioId ==
                                    usuario.Id &&
                                ta.AlumnoId ==
                                    alumnoId) &&
                        _context.UsuariosSedes
                            .Any(us =>
                                us.UsuarioId ==
                                    usuario.Id &&
                                us.SedeId ==
                                    x.Classroom.SiteId));


            if (accesoTerapeuta)
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // CONSULTA / DTO
    // =========================================================


    private IQueryable<EventoCrisisConsulta>
        ConstruirConsultaEventos()
    {
        return
            from evento in
                _context.CrisisEvents
                    .AsNoTracking()

            join alumno in
                _context.Students
                    .AsNoTracking()
                on evento.StudentId
                equals alumno.Id

            join creador in
                _context.Users
                    .AsNoTracking()
                on evento.CreatedByUserId
                equals creador.Id

            select new EventoCrisisConsulta
            {
                Evento =
                    evento,

                Alumno =
                    alumno,

                Creador =
                    creador
            };
    }


    private static EventoCrisisDto CrearDto(
        EventoCrisisConsulta x)
    {
        return new EventoCrisisDto
        {
            Id =
                x.Evento.Id,

            AlumnoId =
                x.Evento.StudentId,

            AlumnoNombre =
                x.Alumno.FirstName +
                " " +
                x.Alumno.LastName,

            CreadoPorUsuarioId =
                x.Evento.CreatedByUserId,

            CreadoPorNombre =
                x.Creador.Nombre +
                " " +
                x.Creador.Apellido,

            FechaHoraEvento =
                x.Evento.EventDateTime,

            DuracionMinutos =
                x.Evento.DurationMinutes,

            Contexto =
                x.Evento.Context,

            DescripcionEvento =
                x.Evento.EventDescription,

            Desencadenante =
                x.Evento.Trigger,

            ApoyoInmediato =
                x.Evento.ImmediateSupport,

            Resultado =
                x.Evento.Outcome,

            SeEstabilizo =
                x.Evento.WasStabilized,

            RequirioApoyoAdicional =
                x.Evento.RequiredAdditionalSupport,

            Notas =
                x.Evento.Notes,

            FechaCreacionUtc =
                x.Evento.CreatedAtUtc
        };
    }


    private async Task<EventoCrisisDto?>
        ObtenerEventoDtoAsync(
            Guid id)
    {
        return await ConstruirConsultaEventos()
            .Where(x =>
                x.Evento.Id ==
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


    private static bool TextoValido(
        string? texto,
        int maximo)
    {
        return string.IsNullOrWhiteSpace(
            texto) ||
            texto.Trim().Length <=
                maximo;
    }


    private sealed class EventoCrisisConsulta
    {
        public CrisisEvent Evento { get; set; }
            = null!;

        public Student Alumno { get; set; }
            = null!;

        public ApplicationUser Creador { get; set; }
            = null!;
    }
}