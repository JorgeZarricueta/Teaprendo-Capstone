using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Citas;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/citas")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;


    public CitasController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // =========================================================
    // APODERADO
    // =========================================================


    // Obtiene solamente los alumnos vinculados al Apoderado.
    [Authorize(Roles = "Apoderado")]
    [HttpGet("alumnos")]
    public async Task<ActionResult<List<AlumnoCitaDto>>>
        ObtenerAlumnos()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var alumnos =
            await _context.ApoderadosAlumnos
                .AsNoTracking()
                .Where(x =>
                    x.UsuarioId == usuario.Id &&
                    x.Alumno.IsActive)
                .OrderBy(x =>
                    x.Alumno.FirstName)
                .ThenBy(x =>
                    x.Alumno.LastName)
                .Select(x =>
                    new AlumnoCitaDto
                    {
                        Id =
                            x.Alumno.Id,

                        Nombre =
                            x.Alumno.FirstName,

                        Apellido =
                            x.Alumno.LastName,

                        AulaNombre =
                            x.Alumno.Enrollments
                                .Where(m =>
                                    m.IsActive)
                                .Select(m =>
                                    m.Classroom.Name)
                                .FirstOrDefault()
                            ?? string.Empty
                    })
                .ToListAsync();


        return Ok(alumnos);
    }


    // Obtiene Docentes y Terapeutas disponibles
    // para un alumno vinculado al Apoderado.
    [Authorize(Roles = "Apoderado")]
    [HttpGet("alumnos/{alumnoId:guid}/profesionales")]
    public async Task<ActionResult<List<ProfesionalCitaDto>>>
        ObtenerProfesionales(
            Guid alumnoId)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        // Verifica que el alumno pertenezca al Apoderado.
        var alumnoAutorizado =
            await _context.ApoderadosAlumnos
                .AnyAsync(x =>
                    x.UsuarioId == usuario.Id &&
                    x.AlumnoId == alumnoId &&
                    x.Alumno.IsActive);

        if (!alumnoAutorizado)
        {
            return Forbid();
        }


        // Matrícula activa actual del alumno.
        var aulaId =
            await _context.Enrollments
                .Where(x =>
                    x.StudentId == alumnoId &&
                    x.IsActive &&
                    x.Classroom.IsActive)
                .Select(x =>
                    (Guid?)x.ClassroomId)
                .FirstOrDefaultAsync();


        var profesionales =
            new List<ProfesionalCitaDto>();


        // Docentes del aula actual.
        if (aulaId.HasValue)
        {
            var docentes =
                await _context.DocentesAulas
                    .AsNoTracking()
                    .Where(x =>
                        x.AulaId == aulaId.Value &&
                        x.Usuario.Activo)
                    .Select(x =>
                        new ProfesionalCitaDto
                        {
                            Id =
                                x.UsuarioId,

                            Nombre =
                                x.Usuario.Nombre,

                            Apellido =
                                x.Usuario.Apellido,

                            Rol =
                                "Docente"
                        })
                    .ToListAsync();


            profesionales.AddRange(
                docentes);
        }


        // Terapeutas asignados directamente al alumno.
        var terapeutas =
            await _context.TerapeutasAlumnos
                .AsNoTracking()
                .Where(x =>
                    x.AlumnoId == alumnoId &&
                    x.Usuario.Activo)
                .Select(x =>
                    new ProfesionalCitaDto
                    {
                        Id =
                            x.UsuarioId,

                        Nombre =
                            x.Usuario.Nombre,

                        Apellido =
                            x.Usuario.Apellido,

                        Rol =
                            "Terapeuta"
                    })
                .ToListAsync();


        profesionales.AddRange(
            terapeutas);


        profesionales =
            profesionales
                .GroupBy(x =>
                    new
                    {
                        x.Id,
                        x.Rol
                    })
                .Select(x =>
                    x.First())
                .OrderBy(x =>
                    x.Rol)
                .ThenBy(x =>
                    x.Nombre)
                .ThenBy(x =>
                    x.Apellido)
                .ToList();


        return Ok(profesionales);
    }


    // Crea una solicitud de cita.
    [Authorize(Roles = "Apoderado")]
    [HttpPost]
    public async Task<ActionResult<CitaDto>> Crear(
        CrearCitaRequest request)
    {
        var apoderado =
            await _userManager.GetUserAsync(User);

        if (apoderado is null)
        {
            return Unauthorized();
        }


        // Valida datos básicos.
        if (request.AlumnoId ==
            Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un alumno."
            });
        }


        if (request.ProfesionalUsuarioId ==
            Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un profesional."
            });
        }


        if (string.IsNullOrWhiteSpace(
            request.Motivo))
        {
            return BadRequest(new
            {
                mensaje =
                    "El motivo de la cita es obligatorio."
            });
        }

        if (request.FechaHoraInicio <=
            DateTime.Now)
        {
            return BadRequest(new
            {
                mensaje =
                    "La fecha de la cita debe ser futura."
            });
        }

        if (request.FechaHoraFin <=
            request.FechaHoraInicio)
        {
            return BadRequest(new
            {
                mensaje =
                    "La hora de término debe ser posterior al inicio."
            });
        }


        // El Apoderado debe estar vinculado al alumno.
        var alumnoAutorizado =
            await _context.ApoderadosAlumnos
                .AnyAsync(x =>
                    x.UsuarioId ==
                        apoderado.Id &&
                    x.AlumnoId ==
                        request.AlumnoId &&
                    x.Alumno.IsActive);

        if (!alumnoAutorizado)
        {
            return Forbid();
        }
        var alumno =
            await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                        request.AlumnoId);


        if (alumno is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Alumno no encontrado."
            });
        }


        // El profesional debe existir y estar activo.
        var profesional =
            await _userManager.FindByIdAsync(
                request
                    .ProfesionalUsuarioId
                    .ToString());

        if (profesional is null ||
            !profesional.Activo)
        {
            return BadRequest(new
            {
                mensaje =
                    "El profesional seleccionado no está disponible."
            });
        }


        var rolesProfesional =
            await _userManager.GetRolesAsync(
                profesional);


        // Aula actual del alumno.
        var aulaId =
            await _context.Enrollments
                .Where(x =>
                    x.StudentId ==
                        request.AlumnoId &&
                    x.IsActive &&
                    x.Student.IsActive &&
                    x.Classroom.IsActive)
                .Select(x =>
                    (Guid?)x.ClassroomId)
                .FirstOrDefaultAsync();


        var esDocenteValido =
            false;

        var esTerapeutaValido =
            false;


        if (rolesProfesional.Contains(
                "Docente",
                StringComparer.OrdinalIgnoreCase) &&
            aulaId.HasValue)
        {
            esDocenteValido =
                await _context.DocentesAulas
                    .AnyAsync(x =>
                        x.UsuarioId ==
                            profesional.Id &&
                        x.AulaId ==
                            aulaId.Value);
        }


        if (rolesProfesional.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            esTerapeutaValido =
                await _context.TerapeutasAlumnos
                    .AnyAsync(x =>
                        x.UsuarioId ==
                            profesional.Id &&
                        x.AlumnoId ==
                            request.AlumnoId);
        }


        if (!esDocenteValido &&
            !esTerapeutaValido)
        {
            return Forbid();
        }


        var rolProfesional =
            esDocenteValido
                ? "Docente"
                : "Terapeuta";


        // Evita enviar exactamente la misma solicitud pendiente.
        var existeSolicitud =
            await _context.Appointments
                .AnyAsync(x =>
                    x.GuardianUserId ==
                        apoderado.Id &&
                    x.StudentId ==
                        request.AlumnoId &&
                    x.ProfessionalUserId ==
                        profesional.Id &&
                    x.RequestedStartDateTime ==
                        request.FechaHoraInicio &&
                    x.Status ==
                        "Pendiente");

        if (existeSolicitud)
        {
            return BadRequest(new
            {
                mensaje =
                    "Ya existe una solicitud pendiente para ese horario."
            });
        }


        var cita =
            new Appointment
            {
                Id =
                    Guid.NewGuid(),

                StudentId =
                    request.AlumnoId,

                GuardianUserId =
                    apoderado.Id,

                ProfessionalUserId =
                    profesional.Id,

                ProfessionalRole =
                    rolProfesional,

                Reason =
                    request.Motivo.Trim(),

                RequestedStartDateTime =
                    request.FechaHoraInicio,

                RequestedEndDateTime =
                    request.FechaHoraFin,

                Status =
                    "Pendiente",

                CreatedAtUtc =
                    DateTime.UtcNow
            };


        _context.Appointments.Add(
            cita);
        // Notifica al profesional sobre
        // la nueva solicitud de cita.
        AgregarNotificacion(
            profesional.Id,
            "CitaSolicitada",
            "Nueva solicitud de cita",
            $"Se solicitó una cita por " +
            $"{alumno.FirstName} {alumno.LastName}.",
            "/profesional/citas",
            cita.Id);


        await _context.SaveChangesAsync();


        var respuesta =
            await ObtenerCitaDtoAsync(
                cita.Id);


        return Ok(respuesta);
    }


    // Solicitudes realizadas por el Apoderado.
    [Authorize(Roles = "Apoderado")]
    [HttpGet("mis-solicitudes")]
    public async Task<ActionResult<List<CitaDto>>>
        ObtenerMisSolicitudes()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var citas =
            await ConstruirConsultaCitas()
                .Where(x =>
                    x.Cita.GuardianUserId ==
                        usuario.Id)
                .OrderByDescending(x =>
                    x.Cita.CreatedAtUtc)
                .Select(x =>
                    CrearDto(x))
                .ToListAsync();


        return Ok(citas);
    }


    // Cancela una solicitud mientras esté pendiente.
    [Authorize(Roles = "Apoderado")]
    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(
        Guid id)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var cita =
            await _context.Appointments
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (cita is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Cita no encontrada."
            });
        }


        if (cita.GuardianUserId !=
            usuario.Id)
        {
            return Forbid();
        }


        if (cita.Status !=
            "Pendiente")
        {
            return BadRequest(new
            {
                mensaje =
                    "Solo se pueden cancelar solicitudes pendientes."
            });
        }


        cita.Status =
            "Cancelada";


        // Notifica al profesional que
        // la solicitud fue cancelada.
        AgregarNotificacion(
            cita.ProfessionalUserId,
            "CitaCancelada",
            "Solicitud de cita cancelada",
            "El apoderado canceló una solicitud de cita.",
            "/profesional/citas",
            cita.Id);


        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                "Solicitud cancelada correctamente."
        });
    }


    // =========================================================
    // DOCENTE / TERAPEUTA
    // =========================================================


    // Solicitudes dirigidas al profesional autenticado.
    [Authorize(Roles = "Docente,Terapeuta")]
    [HttpGet("recibidas")]
    public async Task<ActionResult<List<CitaDto>>>
        ObtenerRecibidas()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var citas =
            await ConstruirConsultaCitas()
                .Where(x =>
                    x.Cita.ProfessionalUserId ==
                        usuario.Id)
                .OrderBy(x =>
                    x.Cita.Status !=
                        "Pendiente")
                .ThenBy(x =>
                    x.Cita.RequestedStartDateTime)
                .Select(x =>
                    CrearDto(x))
                .ToListAsync();


        return Ok(citas);
    }


    // Acepta o rechaza una solicitud.
    [Authorize(Roles = "Docente,Terapeuta")]
    [HttpPatch("{id:guid}/responder")]
    public async Task<IActionResult> Responder(
        Guid id,
        ResponderCitaRequest request)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var cita =
            await _context.Appointments
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (cita is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Cita no encontrada."
            });
        }


        if (cita.ProfessionalUserId !=
            usuario.Id)
        {
            return Forbid();
        }


        if (cita.Status !=
            "Pendiente")
        {
            return BadRequest(new
            {
                mensaje =
                    "Esta solicitud ya fue respondida o cancelada."
            });
        }


        cita.Status =
            request.Aceptar
                ? "Aceptada"
                : "Rechazada";


        cita.ResponseMessage =
            string.IsNullOrWhiteSpace(
                request.Mensaje)
                ? null
                : request.Mensaje.Trim();


        cita.RespondedAtUtc =
            DateTime.UtcNow;

        // Notifica al Apoderado.

        var mensajeNotificacion =
        request.Aceptar
            ? "Tu solicitud de cita fue aceptada."
            : "Tu solicitud de cita fue rechazada.";


        if (!string.IsNullOrWhiteSpace(
            request.Mensaje))
        {
            mensajeNotificacion +=
                $" {request.Mensaje.Trim()}";
        }
        if (request.Aceptar)
        {
            AgregarNotificacion(
                cita.GuardianUserId,
                "CitaAceptada",
                "Cita aceptada",
                mensajeNotificacion,
                "/apoderado/citas",
                cita.Id);
        }
        else
        {
            AgregarNotificacion(
                cita.GuardianUserId,
                "CitaRechazada",
                "Cita rechazada",
                mensajeNotificacion,
                "/apoderado/citas",
                cita.Id);
        }

        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                request.Aceptar
                    ? "Cita aceptada correctamente."
                    : "Cita rechazada correctamente."
        });
    }


    // =========================================================
    // HELPERS
    // =========================================================


    // Base común para construir DTOs con alumno y usuarios.
    private IQueryable<CitaConsulta> ConstruirConsultaCitas()
    {
        return
            from cita in
                _context.Appointments
                    .AsNoTracking()

            join alumno in
                _context.Students
                    .AsNoTracking()
                on cita.StudentId
                equals alumno.Id

            join apoderado in
                _context.Users
                    .AsNoTracking()
                on cita.GuardianUserId
                equals apoderado.Id

            join profesional in
                _context.Users
                    .AsNoTracking()
                on cita.ProfessionalUserId
                equals profesional.Id

            select new CitaConsulta
            {
                Cita =
                    cita,

                Alumno =
                    alumno,

                Apoderado =
                    apoderado,

                Profesional =
                    profesional
            };
    }


    private static CitaDto CrearDto(
        CitaConsulta x)
    {
        return new CitaDto
        {
            Id =
                x.Cita.Id,

            AlumnoId =
                x.Cita.StudentId,

            AlumnoNombre =
                x.Alumno.FirstName +
                " " +
                x.Alumno.LastName,

            ApoderadoUsuarioId =
                x.Cita.GuardianUserId,

            ApoderadoNombre =
                x.Apoderado.Nombre +
                " " +
                x.Apoderado.Apellido,

            ProfesionalUsuarioId =
                x.Cita.ProfessionalUserId,

            ProfesionalNombre =
                x.Profesional.Nombre +
                " " +
                x.Profesional.Apellido,

            RolProfesional =
                x.Cita.ProfessionalRole,

            Motivo =
                x.Cita.Reason,

            FechaHoraInicio =
                x.Cita.RequestedStartDateTime,

            FechaHoraFin =
                x.Cita.RequestedEndDateTime,

            Estado =
                x.Cita.Status,

            MensajeRespuesta =
                x.Cita.ResponseMessage,

            FechaCreacionUtc =
                x.Cita.CreatedAtUtc
        };
    }


    private async Task<CitaDto?>
        ObtenerCitaDtoAsync(
            Guid citaId)
    {
        return await ConstruirConsultaCitas()
            .Where(x =>
                x.Cita.Id == citaId)
            .Select(x =>
                CrearDto(x))
            .FirstOrDefaultAsync();
    }


    // Tipo interno únicamente para facilitar las consultas.
    private class CitaConsulta
    {
        public Appointment Cita { get; set; }
            = null!;

        public Student Alumno { get; set; }
            = null!;

        public ApplicationUser Apoderado { get; set; }
            = null!;

        public ApplicationUser Profesional { get; set; }
            = null!;
    }

    // Crea una notificación interna asociada a una cita.
    private void AgregarNotificacion(
        Guid usuarioId,
        string tipo,
        string titulo,
        string mensaje,
        string ruta,
        Guid citaId)
    {
        var notificacion =
            new Notification
            {
                Id =
                    Guid.NewGuid(),

                UserId =
                    usuarioId,

                Type =
                    tipo,

                Title =
                    titulo,

                Message =
                    mensaje,

                Route =
                    ruta,

                EntityType =
                    "Cita",

                EntityId =
                    citaId,

                IsRead =
                    false,

                CreatedAtUtc =
                    DateTime.UtcNow
            };


        _context.Notifications.Add(
            notificacion);
    }
}