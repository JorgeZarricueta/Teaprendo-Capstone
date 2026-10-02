using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Profesional.Observaciones;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/observaciones")]
[Authorize(Roles = "AdministradorGeneral,Director,Docente,Terapeuta")]
public class ObservacionesController : ControllerBase
{
    private static readonly string[] ContextosPermitidos =
    {
        "Clase",
        "Actividad programada",
        "Recreo",
        "Sesión terapéutica",
        "Transición",
        "Otro"
    };

    private static readonly string[] CategoriasPermitidas =
    {
        "Sensorial",
        "Regulación emocional",
        "Comunicación",
        "Interacción social",
        "Aprendizaje",
        "Autonomía",
        "Otro"
    };

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ObservacionesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("alumno/{alumnoId:guid}")]
    public async Task<ActionResult<List<ObservacionDto>>> ObtenerPorAlumno(
        Guid alumnoId)
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        if (!await PuedeAccederAlumnoAsync(usuario, alumnoId))
        {
            return Forbid();
        }

        return Ok(await CrearConsultaDto(usuario.Id)
            .Where(x => x.AlumnoId == alumnoId && x.Activa)
            .OrderByDescending(x => x.FechaObservacion)
            .ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Docente,Terapeuta")]
    public async Task<ActionResult<ObservacionDto>> Crear(
        CrearObservacionRequest request)
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        if (!await PuedeAccederAlumnoAsync(usuario, request.AlumnoId))
        {
            return Forbid();
        }

        var error = await ValidarAsync(
            request.AlumnoId,
            request.ActividadId,
            request.FechaObservacion,
            request.Contexto,
            request.Categoria,
            request.Situacion,
            request.RespuestaAlumno);

        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        var observacion = new StudentObservation
        {
            Id = Guid.NewGuid(),
            StudentId = request.AlumnoId,
            AuthorUserId = usuario.Id,
            ActivityId = request.ActividadId,
            ObservedAt = request.FechaObservacion,
            Context = Normalizar(request.Contexto, ContextosPermitidos),
            Category = Normalizar(request.Categoria, CategoriasPermitidas),
            Situation = request.Situacion.Trim(),
            Trigger = LimpiarOpcional(request.Desencadenante),
            SupportApplied = LimpiarOpcional(request.ApoyoAplicado),
            StudentResponse = request.RespuestaAlumno.Trim(),
            WasStabilized = request.SeEstabilizo,
            FollowUp = LimpiarOpcional(request.Seguimiento),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.StudentObservations.Add(observacion);
        await _context.SaveChangesAsync();

        var respuesta = await CrearConsultaDto(usuario.Id)
            .FirstAsync(x => x.Id == observacion.Id);

        return CreatedAtAction(
            nameof(ObtenerPorAlumno),
            new { alumnoId = request.AlumnoId },
            respuesta);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Docente,Terapeuta")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarObservacionRequest request)
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var observacion = await _context.StudentObservations
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (observacion is null)
        {
            return NotFound(new { mensaje = "Observación no encontrada." });
        }

        if (observacion.AuthorUserId != usuario.Id ||
            !await PuedeAccederAlumnoAsync(usuario, observacion.StudentId))
        {
            return Forbid();
        }

        var error = await ValidarAsync(
            observacion.StudentId,
            request.ActividadId,
            request.FechaObservacion,
            request.Contexto,
            request.Categoria,
            request.Situacion,
            request.RespuestaAlumno);

        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        observacion.ActivityId = request.ActividadId;
        observacion.ObservedAt = request.FechaObservacion;
        observacion.Context = Normalizar(request.Contexto, ContextosPermitidos);
        observacion.Category = Normalizar(request.Categoria, CategoriasPermitidas);
        observacion.Situation = request.Situacion.Trim();
        observacion.Trigger = LimpiarOpcional(request.Desencadenante);
        observacion.SupportApplied = LimpiarOpcional(request.ApoyoAplicado);
        observacion.StudentResponse = request.RespuestaAlumno.Trim();
        observacion.WasStabilized = request.SeEstabilizo;
        observacion.FollowUp = LimpiarOpcional(request.Seguimiento);
        observacion.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Observación actualizada correctamente." });
    }

    [HttpPatch("{id:guid}/desactivar")]
    [Authorize(Roles = "Docente,Terapeuta")]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var observacion = await _context.StudentObservations
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (observacion is null)
        {
            return NotFound(new { mensaje = "Observación no encontrada." });
        }

        if (observacion.AuthorUserId != usuario.Id)
        {
            return Forbid();
        }

        observacion.IsActive = false;
        observacion.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Observación retirada del historial." });
    }

    private IQueryable<ObservacionProyeccion> CrearConsultaDto(Guid usuarioId)
    {
        return _context.StudentObservations
            .AsNoTracking()
            .Join(
                _context.Users,
                observacion => observacion.AuthorUserId,
                autor => autor.Id,
                (observacion, autor) => new ObservacionProyeccion
                {
                    Id = observacion.Id,
                    AlumnoId = observacion.StudentId,
                    AutorUsuarioId = observacion.AuthorUserId,
                    AutorNombre = autor.Nombre + " " + autor.Apellido,
                    AutorRol = _context.UserRoles
                        .Where(x => x.UserId == autor.Id)
                        .Join(
                            _context.Roles,
                            relacion => relacion.RoleId,
                            rol => rol.Id,
                            (relacion, rol) => rol.Name)
                        .FirstOrDefault() ?? string.Empty,
                    ActividadId = observacion.ActivityId,
                    ActividadTitulo = observacion.Activity != null
                        ? observacion.Activity.Title
                        : null,
                    FechaObservacion = observacion.ObservedAt,
                    Contexto = observacion.Context,
                    Categoria = observacion.Category,
                    Situacion = observacion.Situation,
                    Desencadenante = observacion.Trigger,
                    ApoyoAplicado = observacion.SupportApplied,
                    RespuestaAlumno = observacion.StudentResponse,
                    SeEstabilizo = observacion.WasStabilized,
                    Seguimiento = observacion.FollowUp,
                    FechaCreacionUtc = observacion.CreatedAtUtc,
                    PuedeEditar = observacion.AuthorUserId == usuarioId,
                    Activa = observacion.IsActive
                });
    }

    private async Task<bool> PuedeAccederAlumnoAsync(
        ApplicationUser usuario,
        Guid alumnoId)
    {
        var roles = await _userManager.GetRolesAsync(usuario);

        if (roles.Contains("AdministradorGeneral"))
        {
            return await _context.Students.AnyAsync(x => x.Id == alumnoId);
        }

        if (roles.Contains("Director"))
        {
            return await _context.Enrollments.AnyAsync(x =>
                x.StudentId == alumnoId && x.IsActive &&
                _context.UsuariosSedes.Any(us =>
                    us.UsuarioId == usuario.Id &&
                    us.SedeId == x.Classroom.SiteId));
        }

        if (roles.Contains("Docente"))
        {
            return await _context.Enrollments.AnyAsync(x =>
                x.StudentId == alumnoId && x.IsActive &&
                _context.DocentesAulas.Any(da =>
                    da.UsuarioId == usuario.Id &&
                    da.AulaId == x.ClassroomId));
        }

        if (roles.Contains("Terapeuta"))
        {
            return await _context.Enrollments.AnyAsync(x =>
                x.StudentId == alumnoId && x.IsActive &&
                _context.TerapeutasAlumnos.Any(ta =>
                    ta.UsuarioId == usuario.Id &&
                    ta.AlumnoId == alumnoId) &&
                _context.UsuariosSedes.Any(us =>
                    us.UsuarioId == usuario.Id &&
                    us.SedeId == x.Classroom.SiteId));
        }

        return false;
    }

    private async Task<string?> ValidarAsync(
        Guid alumnoId,
        Guid? actividadId,
        DateTime fecha,
        string contexto,
        string categoria,
        string situacion,
        string respuestaAlumno)
    {
        if (fecha == default || fecha > DateTime.Now.AddMinutes(5))
        {
            return "La fecha de la observación no es válida.";
        }

        if (!ContextosPermitidos.Contains(contexto.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return "El contexto seleccionado no es válido.";
        }

        if (!CategoriasPermitidas.Contains(categoria.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return "La categoría seleccionada no es válida.";
        }

        if (string.IsNullOrWhiteSpace(situacion) || situacion.Trim().Length > 1500)
        {
            return "Describe brevemente qué ocurrió (máximo 1500 caracteres).";
        }

        if (string.IsNullOrWhiteSpace(respuestaAlumno) || respuestaAlumno.Trim().Length > 1500)
        {
            return "Describe la respuesta del alumno (máximo 1500 caracteres).";
        }

        if (actividadId.HasValue &&
            !await _context.Activities.AnyAsync(x =>
                x.Id == actividadId.Value && x.StudentId == alumnoId))
        {
            return "La actividad seleccionada no pertenece al alumno.";
        }

        return null;
    }

    private static string Normalizar(string valor, string[] permitidos)
    {
        return permitidos.First(x =>
            x.Equals(valor.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private class ObservacionProyeccion : ObservacionDto
    {
        public bool Activa { get; set; }
    }
}
