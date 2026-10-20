using System.Net.Http.Json;
using System.Globalization;
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
[Authorize(Roles = "Docente,Terapeuta")]
public class ContextoIaController : ControllerBase
{
    // Palabras que no aportan información útil
    // para recuperar observaciones.
    private static readonly HashSet<string> PalabrasIgnoradas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "para",
            "como",
            "quiero",
            "hacer",
            "esta",
            "este",
            "esto",
            "una",
            "uno",
            "unos",
            "unas",
            "con",
            "del",
            "los",
            "las",
            "por",
            "que",
            "sus",
            "han",
            "alumno",
            "alumnos",
            "estudiante",
            "estudiantes",
            "actividad",
            "actividades",
            "considerar",
            "puedo",
            "podria"
        };


    // Permite relacionar conceptos similares
    // aunque no utilicen exactamente la misma palabra.
    private static readonly string[][] GruposDeTerminosRelacionados =
    [
        [
            "musica",
            "sonido",
            "sonidos",
            "ruido",
            "ruidos",
            "auditivo",
            "auditiva",
            "volumen",
            "grito",
            "gritos"
        ],

        [
            "plasticina",
            "plastilina",
            "textura",
            "texturas",
            "tactil",
            "material",
            "materiales"
        ],

        [
            "color",
            "colores",
            "visual",
            "luminoso",
            "brillante",
            "luz",
            "luces"
        ],

        [
            "transicion",
            "cambio",
            "cambios",
            "rutina",
            "anticipacion"
        ]
    ];


    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly HttpClient
        _clienteIa;


    public ContextoIaController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IHttpClientFactory httpClientFactory)
    {
        _context =
            context;

        _userManager =
            userManager;

            _clienteIa =
        httpClientFactory.CreateClient(
            "TEAprendoIA");
    }


    // =========================================================
    // CONTEXTO DEL AULA
    // =========================================================


    // Prepara únicamente información que el profesional
    // autenticado está autorizado a consultar.
    [HttpPost("aula")]
    public async Task<ActionResult<ContextoAulaIaDto>>
        ObtenerContextoAula(
            SolicitudContextoAulaRequest request)
    {
        // Valida aula.
        if (request.AulaId ==
            Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un aula."
            });
        }


        // Valida pregunta.
        if (string.IsNullOrWhiteSpace(
                request.Pregunta) ||
            request.Pregunta.Trim().Length > 1000)
        {
            return BadRequest(new
            {
                mensaje =
                    "La pregunta es obligatoria y admite hasta 1000 caracteres."
            });
        }


        // Evita enviar demasiados antecedentes.
        if (request.MaxObservaciones is < 1 or > 50)
        {
            return BadRequest(new
            {
                mensaje =
                    "El máximo de observaciones debe estar entre 1 y 50."
            });
        }


        var historial =
            request.Historial ??
            [];


        // Limita el historial del chat.
        if (historial.Count > 6 ||
            historial.Any(x =>
                string.IsNullOrWhiteSpace(
                    x.Texto) ||
                x.Texto.Length > 4000))
        {
            return BadRequest(new
            {
                mensaje =
                    "El historial de conversación no es válido."
            });
        }


        var usuario =
            await _userManager.GetUserAsync(
                User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var roles =
            await _userManager.GetRolesAsync(
                usuario);


        // Busca el aula activa.
        var aula =
            await _context.Classrooms
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        request.AulaId &&
                    x.IsActive)
                .Select(x => new
                {
                    x.Id,

                    x.Name,

                    x.SiteId,

                    SedeNombre =
                        x.Site.Name
                })
                .FirstOrDefaultAsync();


        if (aula is null)
        {
            return NotFound(new
            {
                mensaje =
                    "Aula no encontrada."
            });
        }


        // Obtiene solamente alumnos visibles
        // para el profesional actual.
        var alumnoIds =
            await ObtenerAlumnosAutorizadosAsync(
                usuario.Id,
                roles,
                aula.Id);


        if (alumnoIds is null)
        {
            return Forbid();
        }


        // Obtiene alumnos activos del aula
        // dentro del alcance autorizado.
        var alumnos =
            await _context.Enrollments
                .AsNoTracking()
                .Where(x =>
                    x.ClassroomId ==
                        aula.Id &&
                    x.IsActive &&
                    x.Student.IsActive &&
                    alumnoIds.Contains(
                        x.StudentId))
                .Select(x => new
                {
                    x.StudentId,

                    Nombre =
                        x.Student.FirstName +
                        " " +
                        x.Student.LastName
                })
                .ToListAsync();


        // Recupera observaciones conductuales
        // únicamente de esos alumnos.
        var observaciones =
            await _context
                .BehavioralObservations
                .AsNoTracking()
                .Where(x =>
                    alumnoIds.Contains(
                        x.StudentId))
                .Select(x => new
                {
                    x.Id,

                    x.StudentId,

                    x.ObservationDateTime,

                    x.Context,

                    x.Behavior,

                    x.Antecedent,

                    x.Consequence,

                    x.Notes
                })
                .ToListAsync();


        // Considera preguntas recientes del usuario
        // para mantener continuidad en la recuperación.
        var consultaRecuperacion =
            string.Join(
                ' ',
                historial
                    .Where(x =>
                        x.EsUsuario)
                    .Select(x =>
                        x.Texto)
                    .Append(
                        request.Pregunta));


        var terminos =
            ObtenerTerminos(
                consultaRecuperacion);


        // Calcula qué observaciones tienen mayor
        // relación con la pregunta actual.
        var seleccionadas =
            observaciones
                .Select(x => new
                {
                    Observacion =
                        x,

                    Relevancia =
                        CalcularRelevancia(
                            terminos,
                            x.Context,
                            x.Behavior,
                            x.Antecedent,
                            x.Consequence,
                            x.Notes)
                })
                .Where(x =>
                    terminos.Count == 0 ||
                    x.Relevancia > 0)
                .OrderByDescending(x =>
                    x.Relevancia)
                .ThenByDescending(x =>
                    x.Observacion
                        .ObservationDateTime)
                .Take(
                    request.MaxObservaciones)
                .ToList();


        var respuesta =
            new ContextoAulaIaDto
            {
                AulaId =
                    aula.Id,

                AulaNombre =
                    aula.Name,

                SedeNombre =
                    aula.SedeNombre,

                Alcance =
                    roles.Contains(
                        "Terapeuta",
                        StringComparer.OrdinalIgnoreCase)
                    &&
                    !roles.Contains(
                        "Docente",
                        StringComparer.OrdinalIgnoreCase)

                        ? "Alumnos asignados dentro del aula"

                        : "Aula completa",

                TotalAlumnosConsiderados =
                    alumnos.Count,

                TotalObservacionesDisponibles =
                    observaciones.Count,

                Alumnos =
                    alumnos
                        .Select(alumno =>
                            new AlumnoContextoIaDto
                            {
                                AlumnoId =
                                    alumno.StudentId,

                                Nombre =
                                    alumno.Nombre,

                                Observaciones =
                                    seleccionadas
                                        .Where(x =>
                                            x.Observacion.StudentId ==
                                            alumno.StudentId)
                                        .Select(x =>
                                            new ObservacionContextoIaDto
                                            {
                                                ObservacionId =
                                                    x.Observacion.Id,

                                                Fecha =
                                                    x.Observacion
                                                        .ObservationDateTime,

                                                Contexto =
                                                    x.Observacion.Context,

                                                Conducta =
                                                    x.Observacion.Behavior,

                                                Antecedente =
                                                    x.Observacion.Antecedent,

                                                Consecuencia =
                                                    x.Observacion.Consequence,

                                                Notas =
                                                    x.Observacion.Notes,

                                                Relevancia =
                                                    x.Relevancia
                                            })
                                        .ToList()
                            })
                        .ToList()
            };


        return respuesta;
    }

    // =========================================================
    // CHAT IA
    // =========================================================


    // Prepara el contexto autorizado y lo envía
    // al servicio local TEAprendo.AI.
    [HttpPost("chat/aula")]
    public async Task<ActionResult<RespuestaAsistenteIaDto>>
        ConsultarAsistente(
            SolicitudContextoAulaRequest request)
    {
        // Reutiliza exactamente las mismas reglas
        // de autorización y recuperación del contexto.
        var resultadoContexto =
            await ObtenerContextoAula(
                request);


        if (resultadoContexto.Value is null)
        {
            return resultadoContexto.Result ??
                StatusCode(
                    500,
                    new
                    {
                        mensaje =
                            "No fue posible preparar el contexto."
                    });
        }


        var contexto =
            resultadoContexto.Value;


        if (contexto.TotalAlumnosConsiderados == 0)
        {
            return Ok(
                new RespuestaAsistenteIaDto
                {
                    Respuesta =
                        "No hay alumnos activos autorizados en esta aula.",

                    Modelo =
                        "Sin inferencia"
                });
        }


        try
        {
            var respuesta =
                await _clienteIa.PostAsJsonAsync(
                    "api/inferencia/responder",
                    new SolicitudInferenciaIaDto
                    {
                        Pregunta =
                            request.Pregunta.Trim(),

                        Contexto =
                            contexto,

                        Historial =
                            (request.Historial ?? [])
                                .TakeLast(6)
                                .ToList()
                    });


            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle =
                    await respuesta.Content
                        .ReadAsStringAsync();


                return StatusCode(
                    503,
                    new
                    {
                        mensaje =
                            "El servicio local de IA no pudo generar una respuesta.",

                        detalle
                    });
            }


            var resultado =
                await respuesta.Content
                    .ReadFromJsonAsync<
                        RespuestaAsistenteIaDto>();


            if (resultado is null ||
                string.IsNullOrWhiteSpace(
                    resultado.Respuesta))
            {
                return StatusCode(
                    503,
                    new
                    {
                        mensaje =
                            "El servicio de IA devolvió una respuesta vacía."
                    });
            }


            return Ok(
                resultado);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                503,
                new
                {
                    mensaje =
                        "TEAprendo.AI no está disponible en http://localhost:5270."
                });
        }
    }

    // =========================================================
    // CONTEXTO INDIVIDUAL DEL ALUMNO
    // =========================================================


    // Prepara contexto únicamente del alumno consultado.
    [HttpPost("alumno")]
    public async Task<ActionResult<ContextoAulaIaDto>>
        ObtenerContextoAlumno(
            SolicitudContextoAlumnoRequest request)
    {
        if (request.AlumnoId == Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un alumno."
            });
        }


        if (string.IsNullOrWhiteSpace(
                request.Pregunta) ||
            request.Pregunta.Trim().Length > 1000)
        {
            return BadRequest(new
            {
                mensaje =
                    "La pregunta es obligatoria y admite hasta 1000 caracteres."
            });
        }


        if (request.MaxObservaciones is < 1 or > 50)
        {
            return BadRequest(new
            {
                mensaje =
                    "El máximo de observaciones debe estar entre 1 y 50."
            });
        }


        var historial =
            request.Historial ?? [];


        if (historial.Count > 6 ||
            historial.Any(x =>
                string.IsNullOrWhiteSpace(x.Texto) ||
                x.Texto.Length > 4000))
        {
            return BadRequest(new
            {
                mensaje =
                    "El historial de conversación no es válido."
            });
        }


        var usuario =
            await _userManager.GetUserAsync(User);


        if (usuario is null)
        {
            return Unauthorized();
        }


        var roles =
            await _userManager.GetRolesAsync(
                usuario);


        // Busca la matrícula activa del alumno.
        var alumno =
            await _context.Enrollments
                .AsNoTracking()
                .Where(x =>
                    x.StudentId == request.AlumnoId &&
                    x.IsActive &&
                    x.Student.IsActive)
                .Select(x => new
                {
                    AlumnoId =
                        x.Student.Id,

                    Nombre =
                        x.Student.FirstName +
                        " " +
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
                .FirstOrDefaultAsync();


        if (alumno is null)
        {
            return NotFound(new
            {
                mensaje =
                    "No se encontró una matrícula activa para el alumno."
            });
        }


        var accesoPermitido =
            false;


        // Docente: debe estar asignado al aula del alumno.
        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            accesoPermitido =
                await _context.DocentesAulas
                    .AnyAsync(x =>
                        x.UsuarioId == usuario.Id &&
                        x.AulaId == alumno.AulaId);
        }


        // Terapeuta: debe estar asignado directamente
        // al alumno y tener acceso a su sede.
        if (!accesoPermitido &&
            roles.Contains(
                "Terapeuta",
                StringComparer.OrdinalIgnoreCase))
        {
            var asignado =
                await _context.TerapeutasAlumnos
                    .AnyAsync(x =>
                        x.UsuarioId == usuario.Id &&
                        x.AlumnoId == alumno.AlumnoId);


            var perteneceSede =
                await _context.UsuariosSedes
                    .AnyAsync(x =>
                        x.UsuarioId == usuario.Id &&
                        x.SedeId == alumno.SedeId);


            accesoPermitido =
                asignado &&
                perteneceSede;
        }


        if (!accesoPermitido)
        {
            return Forbid();
        }


        // Recupera únicamente observaciones
        // del alumno consultado.
        var observaciones =
            await _context
                .BehavioralObservations
                .AsNoTracking()
                .Where(x =>
                    x.StudentId ==
                        alumno.AlumnoId)
                .Select(x => new
                {
                    x.Id,

                    x.ObservationDateTime,

                    x.Context,

                    x.Behavior,

                    x.Antecedent,

                    x.Consequence,

                    x.Notes
                })
                .ToListAsync();
        // Recupera los eventos de crisis únicamente
        // del alumno consultado.
        var eventosCrisis =
            await _context
                .CrisisEvents
                .AsNoTracking()
                .Where(x =>
                    x.StudentId ==
                        alumno.AlumnoId)
                .Select(x => new
                {
                    x.Id,
                    x.EventDateTime,
                    x.Context,
                    x.EventDescription,
                    x.Trigger,
                    x.ImmediateSupport,
                    x.Outcome,
                    x.WasStabilized,
                    x.RequiredAdditionalSupport,
                    x.Notes
                })
                .ToListAsync();


        // Usa las preguntas recientes del usuario
        // para conservar contexto conversacional.
        var consultaRecuperacion =
            string.Join(
                ' ',
                historial
                    .Where(x =>
                        x.EsUsuario)
                    .Select(x =>
                        x.Texto)
                    .Append(
                        request.Pregunta));


        var terminos =
            ObtenerTerminos(
                consultaRecuperacion);


        var seleccionadas =
            observaciones
                .Select(x => new
                {
                    Observacion =
                        x,

                    Relevancia =
                        CalcularRelevancia(
                            terminos,
                            x.Context,
                            x.Behavior,
                            x.Antecedent,
                            x.Consequence,
                            x.Notes)
                })
                .Where(x =>
                    terminos.Count == 0 ||
                    x.Relevancia > 0)
                .OrderByDescending(x =>
                    x.Relevancia)
                .ThenByDescending(x =>
                    x.Observacion
                        .ObservationDateTime)
                .Take(
                    request.MaxObservaciones)
                .ToList();

        // Selecciona las crisis relacionadas
        // con la pregunta del profesional.
        var crisisSeleccionadas =
            eventosCrisis
                .Select(x => new
                {
                    Evento =
                        x,

                    Relevancia =
                        CalcularRelevancia(
                            terminos,
                            x.Context,
                            x.EventDescription,
                            x.Trigger,
                            $"{x.ImmediateSupport} {x.Outcome}",
                            x.Notes)
                })
                .Where(x =>
                    terminos.Count == 0 ||
                    x.Relevancia > 0)
                .OrderByDescending(x =>
                    x.Relevancia)
                .ThenByDescending(x =>
                    x.Evento.EventDateTime)
                .Take(
                    request.MaxObservaciones)
                .ToList();

        var contexto =
            new ContextoAulaIaDto
            {
                AulaId =
                    alumno.AulaId,

                AulaNombre =
                    alumno.AulaNombre,

                SedeNombre =
                    alumno.SedeNombre,

                Alcance =
                    "Alumno individual",

                TotalAlumnosConsiderados =
                    1,

                TotalObservacionesDisponibles =
                    observaciones.Count,

                Alumnos =
                [
                    new AlumnoContextoIaDto
                    {
                        AlumnoId =
                            alumno.AlumnoId,

                        Nombre =
                            alumno.Nombre,

                        Observaciones =
                            seleccionadas
                                .Select(x =>
                                    new ObservacionContextoIaDto
                                    {
                                        ObservacionId =
                                            x.Observacion.Id,

                                        Fecha =
                                            x.Observacion
                                                .ObservationDateTime,

                                        Contexto =
                                            x.Observacion.Context,

                                        Conducta =
                                            x.Observacion.Behavior,

                                        Antecedente =
                                            x.Observacion.Antecedent,

                                        Consecuencia =
                                            x.Observacion.Consequence,

                                        Notas =
                                            x.Observacion.Notes,

                                        Relevancia =
                                            x.Relevancia
                                    })
                                .ToList(),

                        EventosCrisis =
                            crisisSeleccionadas
                                .Select(x =>
                                    new EventoCrisisContextoIaDto
                                    {
                                        EventoId =
                                            x.Evento.Id,

                                        Fecha =
                                            x.Evento.EventDateTime,

                                        Contexto =
                                            x.Evento.Context,

                                        Descripcion =
                                            x.Evento.EventDescription,

                                        Desencadenante =
                                            x.Evento.Trigger,

                                        ApoyoInmediato =
                                            x.Evento.ImmediateSupport,

                                        Resultado =
                                            x.Evento.Outcome,

                                        SeEstabilizo =
                                            x.Evento.WasStabilized,

                                        RequirioApoyoAdicional =
                                            x.Evento.RequiredAdditionalSupport,

                                        Notas =
                                            x.Evento.Notes,

                                        Relevancia =
                                            x.Relevancia
                                    })
                                .ToList()
                    }
                ]
            };


        return contexto;
    }

    // Envía el contexto individual del alumno
    // al servicio local TEAprendo.AI.
    [HttpPost("chat/alumno")]
    public async Task<ActionResult<RespuestaAsistenteIaDto>>
        ConsultarAsistenteAlumno(
            SolicitudContextoAlumnoRequest request)
    {
        var resultadoContexto =
            await ObtenerContextoAlumno(
                request);


        if (resultadoContexto.Value is null)
        {
            return resultadoContexto.Result ??
                StatusCode(
                    500,
                    new
                    {
                        mensaje =
                            "No fue posible preparar el contexto del alumno."
                    });
        }


        var contexto =
            resultadoContexto.Value;


        try
        {
            var respuesta =
                await _clienteIa.PostAsJsonAsync(
                    "api/inferencia/responder",
                    new SolicitudInferenciaIaDto
                    {
                        Pregunta =
                            request.Pregunta.Trim(),

                        Contexto =
                            contexto,

                        Historial =
                            (request.Historial ?? [])
                                .TakeLast(6)
                                .ToList()
                    });


            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle =
                    await respuesta.Content
                        .ReadAsStringAsync();


                return StatusCode(
                    503,
                    new
                    {
                        mensaje =
                            "El servicio local de IA no pudo generar una respuesta.",

                        detalle
                    });
            }


            var resultado =
                await respuesta.Content
                    .ReadFromJsonAsync<
                        RespuestaAsistenteIaDto>();


            if (resultado is null ||
                string.IsNullOrWhiteSpace(
                    resultado.Respuesta))
            {
                return StatusCode(
                    503,
                    new
                    {
                        mensaje =
                            "El servicio de IA devolvió una respuesta vacía."
                    });
            }


            return Ok(
                resultado);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                503,
                new
                {
                    mensaje =
                        "TEAprendo.AI no está disponible en http://localhost:5270."
                });
        }
    }

    // =========================================================
    // SEGURIDAD
    // =========================================================


    // Obtiene alumnos visibles según el rol profesional.
    private async Task<List<Guid>?>
        ObtenerAlumnosAutorizadosAsync(
            Guid usuarioId,
            IList<string> roles,
            Guid aulaId)
    {
        // Docente:
        // debe estar asignado directamente al aula.
        if (roles.Contains(
            "Docente",
            StringComparer.OrdinalIgnoreCase))
        {
            var tieneAula =
                await _context.DocentesAulas
                    .AnyAsync(x =>
                        x.UsuarioId ==
                            usuarioId &&
                        x.AulaId ==
                            aulaId);


            if (!tieneAula)
            {
                return null;
            }


            return await _context.Enrollments
                .Where(x =>
                    x.ClassroomId ==
                        aulaId &&
                    x.IsActive &&
                    x.Student.IsActive)
                .Select(x =>
                    x.StudentId)
                .Distinct()
                .ToListAsync();
        }


        // Terapeuta:
        // solamente alumnos asignados directamente.
        if (roles.Contains(
            "Terapeuta",
            StringComparer.OrdinalIgnoreCase))
        {
            var ids =
                await _context.Enrollments
                    .Where(x =>
                        x.ClassroomId ==
                            aulaId &&
                        x.IsActive &&
                        x.Student.IsActive &&
                        _context.TerapeutasAlumnos
                            .Any(ta =>
                                ta.UsuarioId ==
                                    usuarioId &&
                                ta.AlumnoId ==
                                    x.StudentId))
                    .Select(x =>
                        x.StudentId)
                    .Distinct()
                    .ToListAsync();


            // Sin alumnos asignados en el aula,
            // el terapeuta no puede consultar su contexto.
            return ids.Count == 0
                ? null
                : ids;
        }


        return null;
    }


    // =========================================================
    // RECUPERACIÓN POR RELEVANCIA
    // =========================================================


    private static HashSet<string>
        ObtenerTerminos(
            string pregunta)
    {
        var terminos =
            Normalizar(
                pregunta)
                .Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Where(x =>
                    x.Length >= 3 &&
                    !PalabrasIgnoradas
                        .Contains(x))
                .ToHashSet(
                    StringComparer
                        .OrdinalIgnoreCase);


        // Amplía términos relacionados.
        foreach (var grupo in
            GruposDeTerminosRelacionados)
        {
            if (grupo.Any(
                terminos.Contains))
            {
                terminos.UnionWith(
                    grupo);
            }
        }


        return terminos;
    }


    private static int
        CalcularRelevancia(
            HashSet<string> terminos,
            string contexto,
            string conducta,
            string? antecedente,
            string? consecuencia,
            string? notas)
    {
        // Sin términos concretos, permite ordenar
        // las observaciones por fecha.
        if (terminos.Count == 0)
        {
            return 1;
        }


        var puntaje = 0;


        // La conducta y el antecedente tienen
        // mayor importancia para la recuperación.
        puntaje +=
            Coincidencias(
                terminos,
                conducta,
                4);

        puntaje +=
            Coincidencias(
                terminos,
                antecedente,
                4);

        puntaje +=
            Coincidencias(
                terminos,
                consecuencia,
                3);

        puntaje +=
            Coincidencias(
                terminos,
                contexto,
                2);

        puntaje +=
            Coincidencias(
                terminos,
                notas,
                2);


        return puntaje;
    }


    private static int Coincidencias(
        HashSet<string> terminos,
        string? texto,
        int peso)
    {
        if (string.IsNullOrWhiteSpace(
            texto))
        {
            return 0;
        }


        var palabras =
            Normalizar(
                texto)
                .Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .ToHashSet(
                    StringComparer
                        .OrdinalIgnoreCase);


        return terminos.Count(
            palabras.Contains) *
            peso;
    }


    // Elimina tildes, puntuación y diferencias
    // entre mayúsculas y minúsculas.
    private static string Normalizar(
        string valor)
    {
        var descompuesto =
            valor
                .ToLowerInvariant()
                .Normalize(
                    NormalizationForm.FormD);


        var resultado =
            new StringBuilder(
                descompuesto.Length);


        foreach (var caracter in
            descompuesto)
        {
            if (CharUnicodeInfo
                .GetUnicodeCategory(
                    caracter) !=
                UnicodeCategory
                    .NonSpacingMark)
            {
                resultado.Append(
                    char.IsLetterOrDigit(
                        caracter)
                        ? caracter
                        : ' ');
            }
        }


        return resultado
            .ToString()
            .Normalize(
                NormalizationForm.FormC);
    }
}