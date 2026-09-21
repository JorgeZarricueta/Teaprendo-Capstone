using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Instituciones;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/instituciones")]

// Solo el administrador general gestiona instituciones.
[Authorize(Roles = "AdministradorGeneral")]
public class InstitucionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstitucionesController(ApplicationDbContext context)
    {
        _context = context;
    }


    // Obtiene todas las instituciones.
    [HttpGet]
    public async Task<ActionResult<List<InstitucionDto>>> ObtenerTodas()
    {
        var instituciones = await _context.Institutions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new InstitucionDto
            {
                Id = x.Id,
                Nombre = x.Name,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(instituciones);
    }


    // Obtiene una institución por su ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitucionDto>> ObtenerPorId(Guid id)
    {
        var institucion = await _context.Institutions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new InstitucionDto
            {
                Id = x.Id,
                Nombre = x.Name,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (institucion is null)
        {
            return NotFound(new
            {
                mensaje = "Institución no encontrada."
            });
        }

        return Ok(institucion);
    }


    // Crea una nueva institución.
    [HttpPost]
    public async Task<ActionResult<InstitucionDto>> Crear(
        CrearInstitucionRequest request)
    {
        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre es obligatorio."
            });
        }

        // Evita instituciones duplicadas.
        var existe = await _context.Institutions
            .AnyAsync(x => x.Name.ToLower() == nombre.ToLower());

        if (existe)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una institución con ese nombre."
            });
        }

        var institucion = new Institution
        {
            Id = Guid.NewGuid(),
            Name = nombre,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Institutions.Add(institucion);

        await _context.SaveChangesAsync();

        var respuesta = new InstitucionDto
        {
            Id = institucion.Id,
            Nombre = institucion.Name,
            Activa = institucion.IsActive,
            FechaCreacionUtc = institucion.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = institucion.Id },
            respuesta);
    }


    // Modifica el nombre de una institución.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarInstitucionRequest request)
    {
        var institucion =
            await _context.Institutions.FindAsync(id);

        if (institucion is null)
        {
            return NotFound(new
            {
                mensaje = "Institución no encontrada."
            });
        }

        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre es obligatorio."
            });
        }

        institucion.Name = nombre;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Institución actualizada correctamente."
        });
    }


    // Activa o desactiva una institución.
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoInstitucionRequest request)
    {
        var institucion =
            await _context.Institutions.FindAsync(id);

        if (institucion is null)
        {
            return NotFound(new
            {
                mensaje = "Institución no encontrada."
            });
        }

        institucion.IsActive = request.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activa
                ? "Institución activada correctamente."
                : "Institución desactivada correctamente."
        });
    }
}