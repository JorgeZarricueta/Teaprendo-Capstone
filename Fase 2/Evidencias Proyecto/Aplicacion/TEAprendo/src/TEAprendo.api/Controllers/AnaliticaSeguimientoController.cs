using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Api.Services;
using TEAprendo.Contracts.Analitica;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;


namespace TEAprendo.Api.Controllers;


[ApiController]
[Route("api/analitica-seguimiento")]
[Authorize(Roles = "Docente,Terapeuta")]
public class AnaliticaSeguimientoController
    : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly AnaliticaSeguimientoService
        _analitica;


    public AnaliticaSeguimientoController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AnaliticaSeguimientoService analitica)
    {
        _context =
            context;

        _userManager =
            userManager;

        _analitica =
            analitica;
    }


    [HttpGet("alumnos/{alumnoId:guid}")]
    public async Task<ActionResult<IndicadoresSeguimientoDto>>
        ObtenerIndicadoresAlumno(
            Guid alumnoId)
    {
        var usuario =
            await _userManager.GetUserAsync(
                User);


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


        var existe =
            await _context.Students
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == alumnoId &&
                    x.IsActive);


        if (!existe)
        {
            return NotFound(new
            {
                mensaje =
                    "Alumno no encontrado."
            });
        }


        var indicadores =
            await _analitica
                .ObtenerIndicadoresAsync(
                    alumnoId);


        return Ok(
            indicadores);
    }


    private async Task<bool>
        PuedeAccederAlumnoAsync(
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
            return await _context
                .TerapeutasAlumnos
                .AnyAsync(x =>
                    x.UsuarioId ==
                        usuario.Id &&
                    x.AlumnoId ==
                        alumnoId &&
                    x.Alumno.IsActive);
        }


        return false;
    }
}