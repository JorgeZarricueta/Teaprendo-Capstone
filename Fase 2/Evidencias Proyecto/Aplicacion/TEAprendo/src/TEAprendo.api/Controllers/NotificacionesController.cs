using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Notificaciones;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;


    public NotificacionesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context =
            context;

        _userManager =
            userManager;
    }


    // =========================================================
    // BANDEJA
    // =========================================================


    // Devuelve solamente las notificaciones
    // del usuario autenticado.
    [HttpGet]
    public async Task<ActionResult<List<NotificacionDto>>>
        ObtenerNotificaciones()
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var notificaciones =
            await _context.Notifications
                .AsNoTracking()
                .Where(x =>
                    x.UserId ==
                        usuario.Id)
                .OrderBy(x =>
                    x.IsRead)
                .ThenByDescending(x =>
                    x.CreatedAtUtc)
                .Take(100)
                .Select(x =>
                    new NotificacionDto
                    {
                        Id =
                            x.Id,

                        Tipo =
                            x.Type,

                        Titulo =
                            x.Title,

                        Mensaje =
                            x.Message,

                        Ruta =
                            x.Route,

                        EntidadTipo =
                            x.EntityType,

                        EntidadId =
                            x.EntityId,

                        Leida =
                            x.IsRead,

                        FechaCreacionUtc =
                            x.CreatedAtUtc,

                        FechaLecturaUtc =
                            x.ReadAtUtc
                    })
                .ToListAsync();


        return Ok(
            notificaciones);
    }


    // =========================================================
    // RESUMEN
    // =========================================================


    // Devuelve la cantidad de notificaciones no leídas.
    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenNotificacionesDto>>
        ObtenerResumen()
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var cantidad =
            await _context.Notifications
                .CountAsync(x =>
                    x.UserId ==
                        usuario.Id &&
                    !x.IsRead);


        return Ok(
            new ResumenNotificacionesDto
            {
                NoLeidas =
                    cantidad
            });
    }


    // =========================================================
    // MARCAR UNA COMO LEÍDA
    // =========================================================


    [HttpPatch("{id:guid}/leer")]
    public async Task<IActionResult> MarcarComoLeida(
        Guid id)
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var notificacion =
            await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (notificacion is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Notificación no encontrada."
            });
        }


        // Un usuario nunca puede modificar
        // una notificación de otra persona.
        if (notificacion.UserId !=
            usuario.Id)
        {
            return Forbid();
        }


        // Si ya estaba leída no hacemos
        // una modificación innecesaria.
        if (notificacion.IsRead)
        {
            return Ok(new
            {
                mensaje =
                    "La notificación ya estaba marcada como leída."
            });
        }


        notificacion.IsRead =
            true;

        notificacion.ReadAtUtc =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                "Notificación marcada como leída."
        });
    }


    // =========================================================
    // MARCAR TODAS COMO LEÍDAS
    // =========================================================


    [HttpPatch("leer-todas")]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var noLeidas =
            await _context.Notifications
                .Where(x =>
                    x.UserId ==
                        usuario.Id &&
                    !x.IsRead)
                .ToListAsync();


        if (noLeidas.Count == 0)
        {
            return Ok(new
            {
                mensaje =
                    "No tienes notificaciones pendientes."
            });
        }


        var fechaLectura =
            DateTime.UtcNow;


        foreach (var notificacion in noLeidas)
        {
            notificacion.IsRead =
                true;

            notificacion.ReadAtUtc =
                fechaLectura;
        }


        await _context.SaveChangesAsync();


        return Ok(new
        {
            mensaje =
                "Todas las notificaciones fueron marcadas como leídas.",

            cantidad =
                noLeidas.Count
        });
    }
}