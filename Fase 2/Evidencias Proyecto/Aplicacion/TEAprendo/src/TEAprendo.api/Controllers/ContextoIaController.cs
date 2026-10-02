using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.IA;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

namespace TEAprendo.Api.Controllers;

[ApiController]
[Route("api/ia/contexto")]
[Authorize(Roles = "AdministradorGeneral,Director,Docente,Terapeuta")]
public class ContextoIaController : ControllerBase
{
    private static readonly HashSet<string> PalabrasIgnoradas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "para", "como", "quiero", "hacer", "esta", "este", "esto",
            "una", "uno", "unos", "unas", "con", "del", "los", "las",
            "por", "que", "sus", "han", "alumno", "alumnos", "actividad",
            "apoyo", "apoyos", "funciono", "funcionaron", "funcionado"
        };

    private static readonly string[][] GruposDeTerminosRelacionados =
    [
        ["musica", "sonido", "sonidos", "ruido", "ruidos", "auditivo", "auditiva", "volumen", "gritar", "gritos"],
        ["plasticina", "plastilina", "textura", "texturas", "tactil", "material"],
        ["color", "colores", "visual", "luminoso", "brillante"],
        ["transicion", "cambio", "cambios", "rutina", "anticipacion"]
    ];

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpClient _clienteIa;

    public ContextoIaController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _userManager = userManager;
        _clienteIa = httpClientFactory.CreateClient("TEAprendoIA");
    }

    [HttpPost("aula")]
    public async Task<ActionResult<ContextoAulaIaDto>> ObtenerContextoAula(
        SolicitudContextoAulaRequest request)
    {
        if (request.AulaId == Guid.Empty)
        {
            return BadRequest(new { mensaje = "Selecciona un aula." });
        }

        if (string.IsNullOrWhiteSpace(request.Pregunta) ||
            request.Pregunta.Trim().Length > 1000)
        {
            return BadRequest(new
            {
                mensaje = "La pregunta es obligatoria y admite hasta 1000 caracteres."
            });
        }

        if (request.MaxObservaciones is < 1 or > 50)
        {
            return BadRequest(new
            {
                mensaje = "El límite de observaciones debe estar entre 1 y 50."
            });
        }

        if (request.Historial.Count > 6 ||
            request.Historial.Any(x =>
                string.IsNullOrWhiteSpace(x.Texto) || x.Texto.Length > 4000))
        {
            return BadRequest(new { mensaje = "El historial de conversación no es válido." });
        }

        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var aula = await _context.Classrooms
            .AsNoTracking()
            .Where(x => x.Id == request.AulaId && x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.SiteId,
                SedeNombre = x.Site.Name
            })
            .FirstOrDefaultAsync();

        if (aula is null)
        {
            return NotFound(new { mensaje = "Aula no encontrada." });
        }

        var alumnoIds = await ObtenerAlumnosAutorizados(
            usuario.Id,
            roles,
            aula.Id,
            aula.SiteId);

        if (alumnoIds is null)
        {
            return Forbid();
        }

        var alumnos = await _context.Enrollments
            .AsNoTracking()
            .Where(x =>
                x.ClassroomId == aula.Id &&
                x.IsActive &&
                x.Student.IsActive &&
                alumnoIds.Contains(x.StudentId))
            .Select(x => new
            {
                x.StudentId,
                Nombre = x.Student.FirstName + " " + x.Student.LastName
            })
            .ToListAsync();

        var observaciones = await _context.StudentObservations
            .AsNoTracking()
            .Where(x => x.IsActive && alumnoIds.Contains(x.StudentId))
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                x.ObservedAt,
                x.Context,
                x.Category,
                x.Situation,
                x.Trigger,
                x.SupportApplied,
                x.StudentResponse,
                x.WasStabilized,
                x.FollowUp
            })
            .ToListAsync();

        var consultaRecuperacion = string.Join(
            ' ',
            request.Historial
                .Where(x => x.EsUsuario)
                .Select(x => x.Texto)
                .Append(request.Pregunta));
        var terminos = ObtenerTerminos(consultaRecuperacion);
        var seleccionadas = observaciones
            .Select(x => new
            {
                Observacion = x,
                Relevancia = CalcularRelevancia(
                    terminos,
                    x.Category,
                    x.Context,
                    x.Situation,
                    x.Trigger,
                    x.SupportApplied,
                    x.StudentResponse,
                    x.FollowUp)
            })
            .Where(x => terminos.Count == 0 || x.Relevancia > 0)
            .OrderByDescending(x => x.Relevancia)
            .ThenByDescending(x => x.Observacion.ObservedAt)
            .Take(request.MaxObservaciones)
            .ToList();

        var respuesta = new ContextoAulaIaDto
        {
            AulaId = aula.Id,
            AulaNombre = aula.Name,
            SedeNombre = aula.SedeNombre,
            Alcance = roles.Contains("Terapeuta")
                ? "Alumnos asignados dentro del aula"
                : "Aula completa",
            TotalAlumnosConsiderados = alumnos.Count,
            TotalObservacionesDisponibles = observaciones.Count,
            Alumnos = alumnos
                .Select(alumno => new AlumnoContextoIaDto
                {
                    AlumnoId = alumno.StudentId,
                    Nombre = alumno.Nombre,
                    Observaciones = seleccionadas
                        .Where(x => x.Observacion.StudentId == alumno.StudentId)
                        .Select(x => new ObservacionContextoIaDto
                        {
                            ObservacionId = x.Observacion.Id,
                            Fecha = x.Observacion.ObservedAt,
                            Contexto = x.Observacion.Context,
                            Categoria = x.Observacion.Category,
                            Situacion = x.Observacion.Situation,
                            Desencadenante = x.Observacion.Trigger,
                            ApoyoAplicado = x.Observacion.SupportApplied,
                            RespuestaAlumno = x.Observacion.StudentResponse,
                            SeEstabilizo = x.Observacion.WasStabilized,
                            Seguimiento = x.Observacion.FollowUp,
                            Relevancia = x.Relevancia
                        })
                        .ToList()
                })
                .ToList()
        };

        return respuesta;
    }

    [HttpPost("chat/aula")]
    public async Task<ActionResult<RespuestaAsistenteIaDto>> ConsultarAsistente(
        SolicitudContextoAulaRequest request)
    {
        var resultadoContexto = await ObtenerContextoAula(request);

        if (resultadoContexto.Value is null)
        {
            return resultadoContexto.Result ??
                StatusCode(500, new { mensaje = "No fue posible preparar el contexto." });
        }

        if (resultadoContexto.Value.TotalAlumnosConsiderados == 0)
        {
            return Ok(new RespuestaAsistenteIaDto
            {
                Respuesta =
                    "No hay alumnos activos autorizados en esta aula.",
                Modelo = "Sin inferencia"
            });
        }

        var respuestaBasica = CrearRespuestaBasica(
            request.Pregunta,
            resultadoContexto.Value);

        if (respuestaBasica is not null)
        {
            return Ok(respuestaBasica);
        }

        try
        {
            var respuesta = await _clienteIa.PostAsJsonAsync(
                "api/inferencia/responder",
                new SolicitudInferenciaIaDto
                {
                    Pregunta = request.Pregunta.Trim(),
                    Contexto = resultadoContexto.Value,
                    Historial = request.Historial.TakeLast(6).ToList()
                });

            if (!respuesta.IsSuccessStatusCode)
            {
                return StatusCode(503, new
                {
                    mensaje = "El servicio local de IA no pudo generar una respuesta."
                });
            }

            var resultado = await respuesta.Content
                .ReadFromJsonAsync<RespuestaAsistenteIaDto>();

            return resultado is null
                ? StatusCode(503, new { mensaje = "El servicio de IA devolvió una respuesta vacía." })
                : Ok(resultado);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new
            {
                mensaje =
                    "El servicio TEAprendo.AI no está disponible. Inícialo antes de consultar."
            });
        }
    }

    private async Task<List<Guid>?> ObtenerAlumnosAutorizados(
        Guid usuarioId,
        IList<string> roles,
        Guid aulaId,
        Guid sedeId)
    {
        if (roles.Contains("AdministradorGeneral"))
        {
            return await ObtenerAlumnosAula(aulaId);
        }

        if (roles.Contains("Director"))
        {
            var autorizado = await _context.UsuariosSedes.AnyAsync(x =>
                x.UsuarioId == usuarioId && x.SedeId == sedeId);

            return autorizado ? await ObtenerAlumnosAula(aulaId) : null;
        }

        if (roles.Contains("Docente"))
        {
            var autorizado = await _context.DocentesAulas.AnyAsync(x =>
                x.UsuarioId == usuarioId && x.AulaId == aulaId);

            return autorizado ? await ObtenerAlumnosAula(aulaId) : null;
        }

        if (roles.Contains("Terapeuta"))
        {
            var perteneceSede = await _context.UsuariosSedes.AnyAsync(x =>
                x.UsuarioId == usuarioId && x.SedeId == sedeId);

            if (!perteneceSede)
            {
                return null;
            }

            return await _context.Enrollments
                .Where(x =>
                    x.ClassroomId == aulaId &&
                    x.IsActive &&
                    _context.TerapeutasAlumnos.Any(ta =>
                        ta.UsuarioId == usuarioId &&
                        ta.AlumnoId == x.StudentId))
                .Select(x => x.StudentId)
                .ToListAsync();
        }

        return null;
    }

    private Task<List<Guid>> ObtenerAlumnosAula(Guid aulaId)
    {
        return _context.Enrollments
            .Where(x =>
                x.ClassroomId == aulaId &&
                x.IsActive &&
                x.Student.IsActive)
            .Select(x => x.StudentId)
            .ToListAsync();
    }

    private static HashSet<string> ObtenerTerminos(string pregunta)
    {
        var terminos = Normalizar(pregunta)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 3 && !PalabrasIgnoradas.Contains(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var grupo in GruposDeTerminosRelacionados)
        {
            if (grupo.Any(terminos.Contains))
            {
                terminos.UnionWith(grupo);
            }
        }

        return terminos;
    }

    private static int CalcularRelevancia(
        HashSet<string> terminos,
        string categoria,
        string contexto,
        string situacion,
        string? desencadenante,
        string? apoyo,
        string respuesta,
        string? seguimiento)
    {
        if (terminos.Count == 0)
        {
            return 1;
        }

        var puntaje = 0;
        puntaje += Coincidencias(terminos, categoria, 3);
        puntaje += Coincidencias(terminos, contexto, 2);
        puntaje += Coincidencias(terminos, situacion, 4);
        puntaje += Coincidencias(terminos, desencadenante, 3);
        puntaje += Coincidencias(terminos, apoyo, 4);
        puntaje += Coincidencias(terminos, respuesta, 3);
        puntaje += Coincidencias(terminos, seguimiento, 2);
        return puntaje;
    }

    private static int Coincidencias(
        HashSet<string> terminos,
        string? texto,
        int peso)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return 0;
        }

        var palabras = Normalizar(texto)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return terminos.Count(palabras.Contains) * peso;
    }

    private static string Normalizar(string valor)
    {
        var descompuesto = valor
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);

        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) !=
                UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(char.IsLetterOrDigit(caracter) ? caracter : ' ');
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }

    private static RespuestaAsistenteIaDto? CrearRespuestaBasica(
        string pregunta,
        ContextoAulaIaDto contexto)
    {
        var consulta = Normalizar(pregunta);
        var consultaCantidad = new[]
        {
            "cuantos alumnos",
            "cuantos estudiantes",
            "cantidad de alumnos",
            "cantidad de estudiantes",
            "numero de alumnos",
            "numero de estudiantes"
        }.Any(consulta.Contains);

        if (consultaCantidad)
        {
            return new RespuestaAsistenteIaDto
            {
                Respuesta = contexto.TotalAlumnosConsiderados == 1
                    ? $"Tienes 1 estudiante activo autorizado en {contexto.AulaNombre}."
                    : $"Tienes {contexto.TotalAlumnosConsiderados} estudiantes activos autorizados en {contexto.AulaNombre}.",
                Modelo = "Datos de TEAprendo"
            };
        }

        var consultaListado = new[]
        {
            "quienes son mis alumnos",
            "quienes son mis estudiantes",
            "nombres de los alumnos",
            "nombres de los estudiantes",
            "lista de alumnos",
            "lista de estudiantes"
        }.Any(consulta.Contains);

        if (!consultaListado)
        {
            return null;
        }

        var nombres = contexto.Alumnos
            .Select(x => x.Nombre)
            .OrderBy(x => x)
            .ToList();

        return new RespuestaAsistenteIaDto
        {
            Respuesta = $"Los estudiantes activos autorizados en {contexto.AulaNombre} son: {string.Join(", ", nombres)}.",
            Modelo = "Datos de TEAprendo"
        };
    }
}
