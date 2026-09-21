using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Administracion.Directores;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/mi-contexto")]
[Authorize]
public class MiContextoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MiContextoController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // Obtiene las sedes disponibles para el usuario actual.
    [HttpGet("sedes")]
    public async Task<ActionResult<List<SedeAsignadaDto>>> ObtenerSedes()
    {
        var usuario =
            await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        // El Administrador General puede acceder a todas.
        if (User.IsInRole("AdministradorGeneral"))
        {
            var todasLasSedes = await _context.Sites
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SedeAsignadaDto
                {
                    Id = x.Id,
                    Nombre = x.Name,
                    InstitucionNombre = x.Institution.Name
                })
                .ToListAsync();

            return Ok(todasLasSedes);
        }

        // Los demás usuarios solo reciben sus sedes asignadas.
        var sedes = await _context.UsuariosSedes
            .AsNoTracking()
            .Where(x =>
                x.UsuarioId == usuario.Id &&
                x.Sede.IsActive)
            .OrderBy(x => x.Sede.Name)
            .Select(x => new SedeAsignadaDto
            {
                Id = x.Sede.Id,
                Nombre = x.Sede.Name,
                InstitucionNombre = x.Sede.Institution.Name
            })
            .ToListAsync();

        return Ok(sedes);
    }
}