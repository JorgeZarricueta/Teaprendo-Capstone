using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Alumnos;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/alumnos")]
[Authorize(Roles = "AdministradorGeneral,Director")]
public class AlumnosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AlumnosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Comprueba si el usuario administra una sede.
    private async Task<bool> PuedeAdministrarSedeAsync(Guid sedeId)
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


    // Obtiene los alumnos de una sede.
    [HttpGet("sede/{sedeId:guid}")]
    public async Task<ActionResult<List<AlumnoDto>>> ObtenerPorSede(
        Guid sedeId)
    {
        if (!await PuedeAdministrarSedeAsync(sedeId))
        {
            return Forbid();
        }

        var alumnos = await _context.Enrollments
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Classroom.SiteId == sedeId)
            .OrderBy(x => x.Student.FirstName)
            .ThenBy(x => x.Student.LastName)
            .Select(x => new AlumnoDto
            {
                Id = x.Student.Id,
                Nombre = x.Student.FirstName,
                Apellido = x.Student.LastName,
                FechaNacimiento = x.Student.BirthDate,
                Activo = x.Student.IsActive,
                AulaId = x.Classroom.Id,
                AulaNombre = x.Classroom.Name,
                SedeId = x.Classroom.Site.Id,
                SedeNombre = x.Classroom.Site.Name
            })
            .ToListAsync();

        return Ok(alumnos);
    }


    // Obtiene un alumno por ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlumnoDto>> ObtenerPorId(Guid id)
    {
        var alumno = await _context.Enrollments
            .AsNoTracking()
            .Where(x =>
                x.StudentId == id &&
                x.IsActive)
            .Select(x => new AlumnoDto
            {
                Id = x.Student.Id,
                Nombre = x.Student.FirstName,
                Apellido = x.Student.LastName,
                FechaNacimiento = x.Student.BirthDate,
                Activo = x.Student.IsActive,
                AulaId = x.Classroom.Id,
                AulaNombre = x.Classroom.Name,
                SedeId = x.Classroom.Site.Id,
                SedeNombre = x.Classroom.Site.Name
            })
            .FirstOrDefaultAsync();

        if (alumno is null)
        {
            return NotFound(new
            {
                mensaje = "Alumno no encontrado."
            });
        }

        if (!await PuedeAdministrarSedeAsync(alumno.SedeId))
        {
            return Forbid();
        }

        return Ok(alumno);
    }


    // Crea un alumno y su primera matrícula.
    [HttpPost]
    public async Task<ActionResult<AlumnoDto>> Crear(
        CrearAlumnoRequest request)
    {
        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido))
        {
            return BadRequest(new
            {
                mensaje = "Nombre y apellido son obligatorios."
            });
        }

        var aula = await _context.Classrooms
            .Include(x => x.Site)
            .FirstOrDefaultAsync(x =>
                x.Id == request.AulaId &&
                x.IsActive);

        if (aula is null)
        {
            return BadRequest(new
            {
                mensaje = "El aula no existe o está desactivada."
            });
        }

        if (!await PuedeAdministrarSedeAsync(aula.SiteId))
        {
            return Forbid();
        }

        var alumno = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = nombre,
            LastName = apellido,
            BirthDate = request.FechaNacimiento.Date,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var matricula = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = alumno.Id,
            ClassroomId = aula.Id,
            StartDate = DateTime.UtcNow.Date,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Students.Add(alumno);
        _context.Enrollments.Add(matricula);

        await _context.SaveChangesAsync();

        var respuesta = new AlumnoDto
        {
            Id = alumno.Id,
            Nombre = alumno.FirstName,
            Apellido = alumno.LastName,
            FechaNacimiento = alumno.BirthDate,
            Activo = alumno.IsActive,
            AulaId = aula.Id,
            AulaNombre = aula.Name,
            SedeId = aula.SiteId,
            SedeNombre = aula.Site.Name
        };

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = alumno.Id },
            respuesta);
    }


    // Actualiza los datos personales del alumno.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarAlumnoRequest request)
    {
        var matricula = await _context.Enrollments
            .Include(x => x.Student)
            .Include(x => x.Classroom)
            .FirstOrDefaultAsync(x =>
                x.StudentId == id &&
                x.IsActive);

        if (matricula is null)
        {
            return NotFound(new
            {
                mensaje = "Alumno no encontrado."
            });
        }

        if (!await PuedeAdministrarSedeAsync(
            matricula.Classroom.SiteId))
        {
            return Forbid();
        }

        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido))
        {
            return BadRequest(new
            {
                mensaje = "Nombre y apellido son obligatorios."
            });
        }

        matricula.Student.FirstName = nombre;
        matricula.Student.LastName = apellido;
        matricula.Student.BirthDate =
            request.FechaNacimiento.Date;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Alumno actualizado correctamente."
        });
    }


    // Cambia al alumno de aula conservando el historial.
    [HttpPut("{id:guid}/aula")]
    public async Task<IActionResult> CambiarAula(
        Guid id,
        CambiarAulaAlumnoRequest request)
    {
        var matriculaActual = await _context.Enrollments
            .Include(x => x.Classroom)
            .FirstOrDefaultAsync(x =>
                x.StudentId == id &&
                x.IsActive);

        if (matriculaActual is null)
        {
            return NotFound(new
            {
                mensaje = "Matrícula activa no encontrada."
            });
        }

        if (!await PuedeAdministrarSedeAsync(
            matriculaActual.Classroom.SiteId))
        {
            return Forbid();
        }

        var nuevaAula = await _context.Classrooms
            .FirstOrDefaultAsync(x =>
                x.Id == request.AulaId &&
                x.IsActive);

        if (nuevaAula is null)
        {
            return BadRequest(new
            {
                mensaje = "El aula seleccionada no es válida."
            });
        }

        if (!await PuedeAdministrarSedeAsync(nuevaAula.SiteId))
        {
            return Forbid();
        }

        // Evita recrear la matrícula en la misma aula.
        if (matriculaActual.ClassroomId == nuevaAula.Id)
        {
            return BadRequest(new
            {
                mensaje = "El alumno ya pertenece a esa aula."
            });
        }

        // Cierra la matrícula anterior.
        matriculaActual.IsActive = false;
        matriculaActual.EndDate = DateTime.UtcNow.Date;

        // Crea la nueva matrícula.
        var nuevaMatricula = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = id,
            ClassroomId = nuevaAula.Id,
            StartDate = DateTime.UtcNow.Date,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Enrollments.Add(nuevaMatricula);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Alumno cambiado de aula correctamente."
        });
    }


    // Activa o desactiva un alumno.
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoAlumnoRequest request)
    {
        var matricula = await _context.Enrollments
            .Include(x => x.Student)
            .Include(x => x.Classroom)
            .FirstOrDefaultAsync(x =>
                x.StudentId == id &&
                x.IsActive);

        if (matricula is null)
        {
            return NotFound(new
            {
                mensaje = "Alumno no encontrado."
            });
        }

        if (!await PuedeAdministrarSedeAsync(
            matricula.Classroom.SiteId))
        {
            return Forbid();
        }

        matricula.Student.IsActive = request.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activo
                ? "Alumno activado correctamente."
                : "Alumno desactivado correctamente."
        });
    }
}