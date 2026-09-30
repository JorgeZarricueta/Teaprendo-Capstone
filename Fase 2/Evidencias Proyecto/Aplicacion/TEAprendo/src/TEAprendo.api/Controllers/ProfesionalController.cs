using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Profesional;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/profesional")]
[Authorize(Roles = "Docente,Terapeuta")]
public class ProfesionalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfesionalController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Devuelve únicamente la información autorizada
    // para el profesional autenticado.
    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenProfesionalDto>> ObtenerResumen()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var roles =
            await _userManager.GetRolesAsync(usuario);

        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            return Ok(
                await CrearResumenDocente(usuario));
        }

        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            return Ok(
                await CrearResumenTerapeuta(usuario));
        }

        return Forbid();
    }


    // Construye el acceso del Docente desde sus aulas.
    private async Task<ResumenProfesionalDto> CrearResumenDocente(
        ApplicationUser usuario)
    {
        var aulas =
            await _context.DocentesAulas
                .AsNoTracking()
                .Where(x =>
                    x.UsuarioId == usuario.Id &&
                    x.Aula.IsActive)
                .OrderBy(x => x.Aula.Name)
                .Select(x =>
                    new AulaProfesionalDto
                    {
                        Id = x.Aula.Id,
                        Nombre = x.Aula.Name,
                        AnioAcademico =
                            x.Aula.AcademicYear,
                        SedeId =
                            x.Aula.SiteId,
                        SedeNombre =
                            x.Aula.Site.Name
                    })
                .ToListAsync();

        var aulaIds =
            aulas
                .Select(x => x.Id)
                .ToList();

        var alumnos =
            await _context.Enrollments
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Student.IsActive &&
                    aulaIds.Contains(x.ClassroomId))
                .OrderBy(x => x.Student.FirstName)
                .ThenBy(x => x.Student.LastName)
                .Select(x =>
                    new AlumnoProfesionalDto
                    {
                        Id = x.Student.Id,
                        Nombre =
                            x.Student.FirstName,
                        Apellido =
                            x.Student.LastName,
                        AulaId =
                            x.Classroom.Id,
                        AulaNombre =
                            x.Classroom.Name,
                        SedeId =
                            x.Classroom.SiteId,
                        SedeNombre =
                            x.Classroom.Site.Name
                    })
                .ToListAsync();

        alumnos =
            alumnos
                .DistinctBy(x => x.Id)
                .ToList();

        return new ResumenProfesionalDto
        {
            Rol = "Docente",
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            CantidadAulas = aulas.Count,
            CantidadAlumnos = alumnos.Count,
            Aulas = aulas,
            Alumnos = alumnos
        };
    }


    // Construye el acceso del Terapeuta desde
    // los alumnos asignados directamente.
    private async Task<ResumenProfesionalDto> CrearResumenTerapeuta(
        ApplicationUser usuario)
    {
        var sedesUsuario =
            await _context.UsuariosSedes
                .AsNoTracking()
                .Where(x =>
                    x.UsuarioId == usuario.Id)
                .Select(x => x.SedeId)
                .ToListAsync();

        var alumnos =
            await _context.TerapeutasAlumnos
                .AsNoTracking()
                .Where(x =>
                    x.UsuarioId == usuario.Id)
                .Join(
                    _context.Enrollments
                        .Where(x =>
                            x.IsActive &&
                            x.Student.IsActive &&
                            sedesUsuario.Contains(
                                x.Classroom.SiteId)),
                    relacion =>
                        relacion.AlumnoId,
                    matricula =>
                        matricula.StudentId,
                    (relacion, matricula) =>
                        new AlumnoProfesionalDto
                        {
                            Id =
                                matricula.Student.Id,

                            Nombre =
                                matricula.Student.FirstName,

                            Apellido =
                                matricula.Student.LastName,

                            AulaId =
                                matricula.Classroom.Id,

                            AulaNombre =
                                matricula.Classroom.Name,

                            SedeId =
                                matricula.Classroom.SiteId,

                            SedeNombre =
                                matricula.Classroom.Site.Name
                        })
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Apellido)
                .ToListAsync();

        alumnos =
            alumnos
                .DistinctBy(x => x.Id)
                .ToList();

        return new ResumenProfesionalDto
        {
            Rol = "Terapeuta",
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            CantidadAulas = 0,
            CantidadAlumnos = alumnos.Count,
            Alumnos = alumnos
        };
    }

    // Obtiene únicamente los alumnos autorizados
    // para el profesional autenticado.
    [HttpGet("alumnos")]
    public async Task<ActionResult<List<AlumnoProfesionalDto>>> ObtenerAlumnos()
    {
    var usuario =
        await _userManager.GetUserAsync(User);

    if (usuario is null)
    {
        return Unauthorized();
    }

    var roles =
        await _userManager.GetRolesAsync(usuario);

    if (roles.Contains(
        "Docente",
        StringComparer.OrdinalIgnoreCase))
    {
        var resumen =
            await CrearResumenDocente(usuario);

        return Ok(resumen.Alumnos);
    }

    if (roles.Contains(
        "Terapeuta",
        StringComparer.OrdinalIgnoreCase))
    {
        var resumen =
            await CrearResumenTerapeuta(usuario);

        return Ok(resumen.Alumnos);
    }

    return Forbid();
}


    // Obtiene el perfil de un alumno autorizado.
    [HttpGet("alumnos/{alumnoId:guid}")]
    public async Task<ActionResult<PerfilAlumnoProfesionalDto>> ObtenerAlumno(
        Guid alumnoId)
    {
    var usuario =
        await _userManager.GetUserAsync(User);

    if (usuario is null)
    {
        return Unauthorized();
    }

    var roles =
        await _userManager.GetRolesAsync(usuario);

    var accesoPermitido = false;


    // Docente: acceso mediante sus aulas.
    if (roles.Contains(
        "Docente",
        StringComparer.OrdinalIgnoreCase))
    {
        accesoPermitido =
            await _context.Enrollments
                .AnyAsync(x =>
                    x.StudentId == alumnoId &&
                    x.IsActive &&
                    x.Student.IsActive &&
                    _context.DocentesAulas.Any(da =>
                        da.UsuarioId == usuario.Id &&
                        da.AulaId == x.ClassroomId));
    }


    // Terapeuta: acceso mediante asignación directa.
    if (!accesoPermitido &&
        roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
    {
        accesoPermitido =
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
    }


    if (!accesoPermitido)
    {
        return Forbid();
    }


    var perfil =
        await _context.Enrollments
            .AsNoTracking()
            .Where(x =>
                x.StudentId == alumnoId &&
                x.IsActive)
            .Select(x =>
                new PerfilAlumnoProfesionalDto
                {
                    Id = x.Student.Id,
                    Nombre =
                        x.Student.FirstName,
                    Apellido =
                        x.Student.LastName,
                    FechaNacimiento =
                        x.Student.BirthDate,
                    AulaId =
                        x.Classroom.Id,
                    AulaNombre =
                        x.Classroom.Name,
                    SedeId =
                        x.Classroom.SiteId,
                    SedeNombre =
                        x.Classroom.Site.Name
                })
            .FirstOrDefaultAsync();


    if (perfil is null)
    {
        return NotFound(new
        {
            mensaje =
                "No se encontró una matrícula activa para el alumno."
        });
    }

    return Ok(perfil);
}
    // Obtiene las aulas asignadas al Docente autenticado.
    [Authorize(Roles = "Docente")]
    [HttpGet("aulas")]
    public async Task<ActionResult<List<AulaProfesionalDto>>> ObtenerAulas()
    {
    var usuario =
        await _userManager.GetUserAsync(User);

    if (usuario is null)
    {
        return Unauthorized();
    }

    var aulas =
        await _context.DocentesAulas
            .AsNoTracking()
            .Where(x =>
                x.UsuarioId == usuario.Id &&
                x.Aula.IsActive)
            .OrderBy(x => x.Aula.Name)
            .Select(x =>
                new AulaProfesionalDto
                {
                    Id = x.Aula.Id,
                    Nombre = x.Aula.Name,
                    AnioAcademico =
                        x.Aula.AcademicYear,
                    SedeId =
                        x.Aula.SiteId,
                    SedeNombre =
                        x.Aula.Site.Name
                })
            .ToListAsync();

    return Ok(aulas);
}


    // Obtiene un aula únicamente si está asignada
    // al Docente autenticado.
    [Authorize(Roles = "Docente")]
    [HttpGet("aulas/{aulaId:guid}")]
    public async Task<ActionResult<DetalleAulaProfesionalDto>> ObtenerAula(
        Guid aulaId)
{
    var usuario =
        await _userManager.GetUserAsync(User);

    if (usuario is null)
    {
        return Unauthorized();
    }

    // Comprueba que el aula esté asignada al Docente.
    var aulaAutorizada =
        await _context.DocentesAulas
            .AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.AulaId == aulaId &&
                x.Aula.IsActive);

    if (!aulaAutorizada)
    {
        return Forbid();
    }

    var aula =
        await _context.Classrooms
            .AsNoTracking()
            .Where(x =>
                x.Id == aulaId &&
                x.IsActive)
            .Select(x =>
                new DetalleAulaProfesionalDto
                {
                    Id = x.Id,
                    Nombre = x.Name,
                    AnioAcademico =
                        x.AcademicYear,
                    SedeId =
                        x.SiteId,
                    SedeNombre =
                        x.Site.Name
                })
            .FirstOrDefaultAsync();

    if (aula is null)
    {
        return NotFound(new
        {
            mensaje = "Aula no encontrada."
        });
    }

    aula.Alumnos =
        await _context.Enrollments
            .AsNoTracking()
            .Where(x =>
                x.ClassroomId == aulaId &&
                x.IsActive &&
                x.Student.IsActive)
            .OrderBy(x => x.Student.FirstName)
            .ThenBy(x => x.Student.LastName)
            .Select(x =>
                new AlumnoProfesionalDto
                {
                    Id = x.Student.Id,
                    Nombre =
                        x.Student.FirstName,
                    Apellido =
                        x.Student.LastName,
                    AulaId =
                        x.Classroom.Id,
                    AulaNombre =
                        x.Classroom.Name,
                    SedeId =
                        x.Classroom.SiteId,
                    SedeNombre =
                        x.Classroom.Site.Name
                })
            .ToListAsync();

    return Ok(aula);
}
}