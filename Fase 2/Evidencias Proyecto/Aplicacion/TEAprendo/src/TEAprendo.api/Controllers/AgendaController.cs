using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Agenda;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/agenda")]
[Authorize(Roles = "Docente,Terapeuta")]
public class AgendaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly string[] TiposPermitidos =
    {
        "Pedagógica",
        "Terapéutica",
        "Seguimiento",
        "Reunión",
        "Otra"
    };


    public AgendaController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Comprueba si el profesional puede acceder al alumno.
    private async Task<bool> PuedeAccederAlumnoAsync(
        ApplicationUser usuario,
        Guid alumnoId)
    {
        var roles =
            await _userManager.GetRolesAsync(usuario);


        // Docente: acceso mediante sus aulas.
        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            var autorizado =
                await _context.Enrollments
                    .AnyAsync(x =>
                        x.StudentId == alumnoId &&
                        x.IsActive &&
                        x.Student.IsActive &&
                        _context.DocentesAulas.Any(da =>
                            da.UsuarioId == usuario.Id &&
                            da.AulaId == x.ClassroomId));

            if (autorizado)
            {
                return true;
            }
        }


        // Terapeuta: acceso mediante asignación directa.
        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var autorizado =
                await _context.Enrollments
                    .AnyAsync(x =>
                        x.StudentId == alumnoId &&
                        x.IsActive &&
                        x.Student.IsActive &&
                        _context.TerapeutasAlumnos.Any(ta =>
                            ta.UsuarioId == usuario.Id &&
                            ta.AlumnoId == alumnoId) &&
                        _context.UsuariosSedes.Any(us =>
                            us.UsuarioId == usuario.Id &&
                            us.SedeId == x.Classroom.SiteId));

            if (autorizado)
            {
                return true;
            }
        }

        return false;
    }


    // Obtiene la agenda autorizada del profesional.
    [HttpGet]
    public async Task<ActionResult<List<ActividadDto>>> ObtenerAgenda(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var alumnosAutorizados =
            await ObtenerAlumnoIdsAutorizadosAsync(
                usuario);


        var consulta =
            _context.Activities
                .AsNoTracking()
                .Where(x =>
                    alumnosAutorizados.Contains(
                        x.StudentId));


        // Permite consultar un rango de fechas.
        if (desde.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.StartDateTime >= desde.Value);
        }

        if (hasta.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.StartDateTime <= hasta.Value);
        }


        var actividades =
            await consulta
                .OrderBy(x =>
                    x.StartDateTime)
                .Select(x =>
                    new ActividadDto
                    {
                        Id = x.Id,

                        AlumnoId =
                            x.Student.Id,

                        AlumnoNombre =
                            x.Student.FirstName +
                            " " +
                            x.Student.LastName,

                        AulaNombre =
                            x.Student.Enrollments
                                .Where(m =>
                                    m.IsActive)
                                .Select(m =>
                                    m.Classroom.Name)
                                .FirstOrDefault()
                            ?? string.Empty,

                        Titulo =
                            x.Title,

                        Descripcion =
                            x.Description,

                        Tipo =
                            x.Type,

                        FechaHoraInicio =
                            x.StartDateTime,

                        FechaHoraFin =
                            x.EndDateTime,

                        Activa =
                            x.IsActive
                    })
                .ToListAsync();


        return Ok(actividades);
    }


    // Obtiene las actividades de un alumno autorizado.
    [HttpGet("alumno/{alumnoId:guid}")]
    public async Task<ActionResult<List<ActividadDto>>> ObtenerPorAlumno(
        Guid alumnoId)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        if (!await PuedeAccederAlumnoAsync(
            usuario,
            alumnoId))
        {
            return Forbid();
        }


        var actividades =
            await _context.Activities
                .AsNoTracking()
                .Where(x =>
                    x.StudentId == alumnoId)
                .OrderByDescending(x =>
                    x.StartDateTime)
                .Select(x =>
                    new ActividadDto
                    {
                        Id = x.Id,

                        AlumnoId =
                            x.Student.Id,

                        AlumnoNombre =
                            x.Student.FirstName +
                            " " +
                            x.Student.LastName,

                        AulaNombre =
                            x.Student.Enrollments
                                .Where(m =>
                                    m.IsActive)
                                .Select(m =>
                                    m.Classroom.Name)
                                .FirstOrDefault()
                            ?? string.Empty,

                        Titulo =
                            x.Title,

                        Descripcion =
                            x.Description,

                        Tipo =
                            x.Type,

                        FechaHoraInicio =
                            x.StartDateTime,

                        FechaHoraFin =
                            x.EndDateTime,

                        Activa =
                            x.IsActive
                    })
                .ToListAsync();


        return Ok(actividades);
    }


    // Crea una actividad para un alumno autorizado.
    [HttpPost]
    public async Task<ActionResult<ActividadDto>> Crear(
        CrearActividadRequest request)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        if (!await PuedeAccederAlumnoAsync(
            usuario,
            request.AlumnoId))
        {
            return Forbid();
        }


        var error =
            ValidarActividad(
                request.Titulo,
                request.Tipo,
                request.FechaHoraInicio,
                request.FechaHoraFin);

        if (error is not null)
        {
            return BadRequest(new
            {
                mensaje = error
            });
        }


        var actividad =
            new Activity
            {
                Id = Guid.NewGuid(),

                StudentId =
                    request.AlumnoId,

                CreatedByUserId =
                    usuario.Id,

                Title =
                    request.Titulo.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Descripcion)
                        ? null
                        : request.Descripcion.Trim(),

                Type =
                    NormalizarTipo(
                        request.Tipo),

                StartDateTime =
                    request.FechaHoraInicio,

                EndDateTime =
                    request.FechaHoraFin,

                IsActive = true,

                CreatedAtUtc =
                    DateTime.UtcNow
            };


        _context.Activities.Add(
            actividad);

        await _context.SaveChangesAsync();


        var respuesta =
            await CrearDtoAsync(
                actividad.Id);


        return CreatedAtAction(
            nameof(ObtenerPorAlumno),
            new
            {
                alumnoId =
                    actividad.StudentId
            },
            respuesta);
    }


    // Edita una actividad existente.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarActividadRequest request)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var actividad =
            await _context.Activities
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (actividad is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Actividad no encontrada."
            });
        }


        if (!await PuedeAccederAlumnoAsync(
            usuario,
            actividad.StudentId))
        {
            return Forbid();
        }


        var error =
            ValidarActividad(
                request.Titulo,
                request.Tipo,
                request.FechaHoraInicio,
                request.FechaHoraFin);

        if (error is not null)
        {
            return BadRequest(new
            {
                mensaje = error
            });
        }


        actividad.Title =
            request.Titulo.Trim();

        actividad.Description =
            string.IsNullOrWhiteSpace(
                request.Descripcion)
                ? null
                : request.Descripcion.Trim();

        actividad.Type =
            NormalizarTipo(
                request.Tipo);

        actividad.StartDateTime =
            request.FechaHoraInicio;

        actividad.EndDateTime =
            request.FechaHoraFin;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                "Actividad actualizada correctamente."
        });
    }


    // Activa o cancela una actividad.
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoActividadRequest request)
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }


        var actividad =
            await _context.Activities
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (actividad is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Actividad no encontrada."
            });
        }


        if (!await PuedeAccederAlumnoAsync(
            usuario,
            actividad.StudentId))
        {
            return Forbid();
        }


        actividad.IsActive =
            request.Activa;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                request.Activa
                    ? "Actividad activada correctamente."
                    : "Actividad cancelada correctamente."
        });
    }


    // Obtiene los alumnos disponibles para el usuario.
    private async Task<List<Guid>>
        ObtenerAlumnoIdsAutorizadosAsync(
            ApplicationUser usuario)
    {
        var roles =
            await _userManager.GetRolesAsync(
                usuario);

        var alumnoIds =
            new HashSet<Guid>();


        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            var alumnosDocente =
                await _context.Enrollments
                    .Where(x =>
                        x.IsActive &&
                        x.Student.IsActive &&
                        _context.DocentesAulas.Any(da =>
                            da.UsuarioId ==
                                usuario.Id &&
                            da.AulaId ==
                                x.ClassroomId))
                    .Select(x =>
                        x.StudentId)
                    .Distinct()
                    .ToListAsync();

            alumnoIds.UnionWith(
                alumnosDocente);
        }


        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var sedes =
                await _context.UsuariosSedes
                    .Where(x =>
                        x.UsuarioId ==
                            usuario.Id)
                    .Select(x =>
                        x.SedeId)
                    .ToListAsync();


            var alumnosTerapeuta =
                await _context.TerapeutasAlumnos
                    .Where(x =>
                        x.UsuarioId ==
                            usuario.Id)
                    .Join(
                        _context.Enrollments
                            .Where(x =>
                                x.IsActive &&
                                x.Student.IsActive &&
                                sedes.Contains(
                                    x.Classroom.SiteId)),
                        relacion =>
                            relacion.AlumnoId,
                        matricula =>
                            matricula.StudentId,
                        (relacion, matricula) =>
                            matricula.StudentId)
                    .Distinct()
                    .ToListAsync();

            alumnoIds.UnionWith(
                alumnosTerapeuta);
        }


        return alumnoIds.ToList();
    }


    // Valida los datos comunes de una actividad.
    private string? ValidarActividad(
        string titulo,
        string tipo,
        DateTime inicio,
        DateTime? fin)
    {
        if (string.IsNullOrWhiteSpace(
            titulo))
        {
            return
                "El título es obligatorio.";
        }


        if (!TiposPermitidos.Contains(
            tipo.Trim(),
            StringComparer.OrdinalIgnoreCase))
        {
            return
                "El tipo de actividad no es válido.";
        }


        if (fin.HasValue &&
            fin.Value < inicio)
        {
            return
                "La fecha de término no puede ser anterior al inicio.";
        }


        return null;
    }


    // Mantiene nombres consistentes para los tipos.
    private string NormalizarTipo(
        string tipo)
    {
        return TiposPermitidos
            .First(x =>
                x.Equals(
                    tipo.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }


    // Construye el DTO después de crear una actividad.
    private async Task<ActividadDto?>
        CrearDtoAsync(Guid actividadId)
    {
        return await _context.Activities
            .AsNoTracking()
            .Where(x =>
                x.Id == actividadId)
            .Select(x =>
                new ActividadDto
                {
                    Id = x.Id,

                    AlumnoId =
                        x.Student.Id,

                    AlumnoNombre =
                        x.Student.FirstName +
                        " " +
                        x.Student.LastName,

                    AulaNombre =
                        x.Student.Enrollments
                            .Where(m =>
                                m.IsActive)
                            .Select(m =>
                                m.Classroom.Name)
                            .FirstOrDefault()
                        ?? string.Empty,

                    Titulo =
                        x.Title,

                    Descripcion =
                        x.Description,

                    Tipo =
                        x.Type,

                    FechaHoraInicio =
                        x.StartDateTime,

                    FechaHoraFin =
                        x.EndDateTime,

                    Activa =
                        x.IsActive
                })
            .FirstOrDefaultAsync();
    }
}