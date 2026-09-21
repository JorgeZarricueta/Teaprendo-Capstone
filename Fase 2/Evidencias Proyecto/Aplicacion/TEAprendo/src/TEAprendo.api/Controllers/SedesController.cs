using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Sedes;
using TEAprendo.Domain.Entities;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/sedes")]

// Solo el administrador general gestiona las sedes.
[Authorize(Roles = "AdministradorGeneral")]
public class SedesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SedesController(ApplicationDbContext context)
    {
        _context = context;
    }


    // Obtiene todas las sedes registradas.
    [HttpGet]
    public async Task<ActionResult<List<SedeDto>>> ObtenerTodas()
    {
        var sedes = await _context.Sites
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SedeDto
            {
                Id = x.Id,
                InstitucionId = x.InstitutionId,
                InstitucionNombre = x.Institution.Name,
                Nombre = x.Name,
                Direccion = x.Address,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(sedes);
    }


    // Obtiene una sede específica.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SedeDto>> ObtenerPorId(Guid id)
    {
        var sede = await _context.Sites
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SedeDto
            {
                Id = x.Id,
                InstitucionId = x.InstitutionId,
                InstitucionNombre = x.Institution.Name,
                Nombre = x.Name,
                Direccion = x.Address,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (sede is null)
        {
            return NotFound(new
            {
                mensaje = "Sede no encontrada."
            });
        }

        return Ok(sede);
    }


    // Obtiene las sedes pertenecientes a una institución.
    [HttpGet("institucion/{institucionId:guid}")]
    public async Task<ActionResult<List<SedeDto>>> ObtenerPorInstitucion(
        Guid institucionId)
    {
        var institucionExiste = await _context.Institutions
            .AnyAsync(x => x.Id == institucionId);

        if (!institucionExiste)
        {
            return NotFound(new
            {
                mensaje = "Institución no encontrada."
            });
        }

        var sedes = await _context.Sites
            .AsNoTracking()
            .Where(x => x.InstitutionId == institucionId)
            .OrderBy(x => x.Name)
            .Select(x => new SedeDto
            {
                Id = x.Id,
                InstitucionId = x.InstitutionId,
                InstitucionNombre = x.Institution.Name,
                Nombre = x.Name,
                Direccion = x.Address,
                Activa = x.IsActive,
                FechaCreacionUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(sedes);
    }


    // Crea una sede dentro de una institución.
    [HttpPost]
    public async Task<ActionResult<SedeDto>> Crear(
        CrearSedeRequest request)
    {
        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre de la sede es obligatorio."
            });
        }

        // Verifica que la institución exista y esté activa.
        var institucion = await _context.Institutions
            .FirstOrDefaultAsync(x =>
                x.Id == request.InstitucionId &&
                x.IsActive);

        if (institucion is null)
        {
            return BadRequest(new
            {
                mensaje = "La institución no existe o está desactivada."
            });
        }

        // Evita nombres repetidos dentro de la misma institución.
        var sedeExiste = await _context.Sites
            .AnyAsync(x =>
                x.InstitutionId == request.InstitucionId &&
                x.Name.ToLower() == nombre.ToLower());

        if (sedeExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una sede con ese nombre en la institución."
            });
        }

        var sede = new Site
        {
            Id = Guid.NewGuid(),
            InstitutionId = request.InstitucionId,
            Name = nombre,
            Address = request.Direccion?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Sites.Add(sede);

        await _context.SaveChangesAsync();

        var respuesta = new SedeDto
        {
            Id = sede.Id,
            InstitucionId = sede.InstitutionId,
            InstitucionNombre = institucion.Name,
            Nombre = sede.Name,
            Direccion = sede.Address,
            Activa = sede.IsActive,
            FechaCreacionUtc = sede.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = sede.Id },
            respuesta);
    }


    // Modifica el nombre y dirección de una sede.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarSedeRequest request)
    {
        var sede = await _context.Sites
            .FindAsync(id);

        if (sede is null)
        {
            return NotFound(new
            {
                mensaje = "Sede no encontrada."
            });
        }

        var nombre = request.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre de la sede es obligatorio."
            });
        }

        // Comprueba duplicados en la misma institución.
        var sedeExiste = await _context.Sites
            .AnyAsync(x =>
                x.Id != id &&
                x.InstitutionId == sede.InstitutionId &&
                x.Name.ToLower() == nombre.ToLower());

        if (sedeExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una sede con ese nombre."
            });
        }

        sede.Name = nombre;
        sede.Address = request.Direccion?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Sede actualizada correctamente."
        });
    }


    // Activa o desactiva una sede.
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoSedeRequest request)
    {
        var sede = await _context.Sites
            .FindAsync(id);

        if (sede is null)
        {
            return NotFound(new
            {
                mensaje = "Sede no encontrada."
            });
        }

        sede.IsActive = request.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activa
                ? "Sede activada correctamente."
                : "Sede desactivada correctamente."
        });
    }
}