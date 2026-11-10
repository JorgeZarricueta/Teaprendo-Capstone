using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEAprendo.Contracts.IA;


var builder = WebApplication.CreateBuilder(args);


// Cliente utilizado para comunicarse con Ollama.
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress =
        new Uri(
            builder.Configuration["Ollama:Url"] ??
            "http://localhost:11434");

    client.Timeout =
        TimeSpan.FromMinutes(5);
});


var app =
    builder.Build();


// Comprueba que TEAprendo.AI está disponible.
app.MapGet(
    "/health",
    () => Results.Ok(new
    {
        estado = "disponible"
    }));


// Recibe contexto preparado por TEAprendo.Api
// y solicita una respuesta al modelo local.
app.MapPost(
    "/api/inferencia/responder",
    async (
        SolicitudInferenciaIaDto solicitud,
        IHttpClientFactory fabrica,
        IConfiguration configuracion) =>
    {
        if (string.IsNullOrWhiteSpace(
                solicitud.Pregunta) ||
            solicitud.Contexto is null ||
            solicitud.Contexto.Alumnos.Count == 0)
        {
            return Results.BadRequest(new
            {
                mensaje =
                    "La pregunta y el contexto son obligatorios."
            });
        }


        var modelo =
            configuracion["Ollama:Modelo"] ??
            "qwen3:4b-q4_K_M";


        // Guarda por separado las fuentes utilizadas.
        var fuentesObservaciones =
            solicitud.Contexto.Alumnos
                .SelectMany(x =>
                    x.Observaciones)
                .Select(x =>
                    x.ObservacionId)
                .Distinct()
                .ToList();


        var fuentesCrisis =
            solicitud.Contexto.Alumnos
                .SelectMany(x =>
                    x.EventosCrisis)
                .Select(x =>
                    x.EventoId)
                .Distinct()
                .ToList();


        var fuentesIntervenciones =
            solicitud.Contexto.Alumnos
                .SelectMany(x =>
                    x.Intervenciones)
                .Select(x =>
                    x.IntervencionId)
                .Distinct()
                .ToList();


        var fuentesResultados =
            solicitud.Contexto.Alumnos
                .SelectMany(x =>
                    x.Intervenciones)
                .SelectMany(x =>
                    x.Resultados)
                .Select(x =>
                    x.ResultadoId)
                .Distinct()
                .ToList();


        var hayIndicadores =
            solicitud.Contexto.Alumnos
                .Any(x =>
                    x.IndicadoresSeguimiento is not null);


        var hayFuentes =
            fuentesObservaciones.Count > 0 ||
            fuentesCrisis.Count > 0 ||
            fuentesIntervenciones.Count > 0 ||
            fuentesResultados.Count > 0 ||
            hayIndicadores;


        var preguntaActual =
            solicitud.Pregunta
                .ToLowerInvariant();


        var consultaPatrones =
            preguntaActual.Contains("patron") ||
            preguntaActual.Contains("patrón") ||
            preguntaActual.Contains("patrones") ||
            preguntaActual.Contains("recurrent") ||
            preguntaActual.Contains("recurrencia") ||
            preguntaActual.Contains("tendencia");


        var consultaEstrategias =
            preguntaActual.Contains("estrategia") ||
            preguntaActual.Contains("intervencion") ||
            preguntaActual.Contains("intervención") ||
            preguntaActual.Contains("efectiv") ||
            preguntaActual.Contains("resultado") ||
            preguntaActual.Contains("funciono") ||
            preguntaActual.Contains("funcionó");


        var tipoConsulta =
            consultaPatrones
                ? "patrones"
                : consultaEstrategias
                    ? "estrategias"
                    : "antecedentes";

        // Solo envía al modelo los alumnos
        // con antecedentes relevantes.
        var contextoParaModelo =
            new
            {
                solicitud.Contexto.AulaNombre,

                solicitud.Contexto.SedeNombre,

                solicitud.Contexto.Alcance,

                solicitud.Contexto
                    .TotalAlumnosConsiderados,

                TipoConsulta =
                    tipoConsulta,

                Alumnos =
                    solicitud.Contexto.Alumnos
                        .Where(x =>
                            x.Observaciones.Count > 0 ||
                            x.EventosCrisis.Count > 0 ||
                            x.Intervenciones.Count > 0 ||
                            x.IndicadoresSeguimiento is not null)
                        .Select(x =>
                            new
                            {
                                x.AlumnoId,

                                x.Nombre,

                                // Los patrones calculados son suficientes
                                // para consultas exclusivamente de recurrencia.
                                Observaciones =
                                    consultaPatrones
                                        ? x.Observaciones
                                            .Take(0)
                                            .ToList()
                                        : consultaEstrategias
                                            ? x.Observaciones
                                                .Take(0)
                                                .ToList()
                                            : x.Observaciones,

                                EventosCrisis =
                                    consultaPatrones
                                        ? x.EventosCrisis
                                            .Take(0)
                                            .ToList()
                                        : consultaEstrategias
                                            ? x.EventosCrisis
                                                .Take(0)
                                                .ToList()
                                            : x.EventosCrisis,

                                // Para efectividad se entregan
                                // las intervenciones y sus resultados.
                                Intervenciones =
                                    consultaEstrategias
                                        ? x.Intervenciones
                                        : x.Intervenciones
                                            .Take(0)
                                            .ToList(),

                                IndicadoresSeguimiento =
                                    x.IndicadoresSeguimiento is null
                                        ? null
                                        : new
                                        {
                                            x.IndicadoresSeguimiento
                                                .TotalObservaciones,

                                            x.IndicadoresSeguimiento
                                                .TotalEventosCrisis,

                                            x.IndicadoresSeguimiento
                                                .TotalIntervenciones,

                                            x.IndicadoresSeguimiento
                                                .TotalResultados,

                                            x.IndicadoresSeguimiento
                                                .ResultadosAlta,

                                            x.IndicadoresSeguimiento
                                                .ResultadosMedia,

                                            x.IndicadoresSeguimiento
                                                .ResultadosBaja,

                                            x.IndicadoresSeguimiento
                                                .SuficienciaContexto,

                                            x.IndicadoresSeguimiento
                                                .SuficienciaEfectividad,

                                            Patrones =
                                                consultaPatrones
                                                    ? x.IndicadoresSeguimiento
                                                        .Patrones
                                                    : x.IndicadoresSeguimiento
                                                        .Patrones
                                                        .Take(0)
                                                        .ToList(),

                                            Estrategias =
                                                consultaEstrategias
                                                    ? x.IndicadoresSeguimiento
                                                        .Estrategias
                                                    : x.IndicadoresSeguimiento
                                                        .Estrategias
                                                        .Take(0)
                                                        .ToList()
                                        }
                            })
                        .ToList()
            };


        var contextoJson =
            JsonSerializer.Serialize(
                contextoParaModelo,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase,

                    WriteIndented =
                        false
                });


        // Instrucciones principales del asistente.
        var promptSistema =
            """
            Eres un asistente de apoyo educativo para docentes y terapeutas
            que utiliza exclusivamente información autorizada proporcionada
            por TEAprendo.

            Tu función es apoyar la interpretación de registros educativos,
            resumir antecedentes, describir recurrencias y presentar
            orientaciones prudentes.

            No diagnostiques.
            No atribuyas trastornos.
            No sustituyas decisiones profesionales.
            No establezcas causalidad clínica.

            Para afirmar hechos sobre estudiantes utiliza exclusivamente
            los antecedentes proporcionados por TEAprendo.

            Nunca inventes conductas, antecedentes, consecuencias,
            características personales, estados emocionales ni reacciones
            que no estén registradas.


            TIPOS DE INFORMACIÓN DISPONIBLES

            TEAprendo puede proporcionar:

            - observaciones:
            registros habituales de seguimiento;

            - eventosCrisis:
            eventos puntuales registrados como situaciones de crisis;

            - intervenciones:
            estrategias aplicadas por profesionales;

            - resultados:
            resultados registrados posteriormente para una intervención;

            - indicadoresSeguimiento:
            información analítica calculada previamente por TEAprendo
            mediante reglas determinísticas.


            DISTINCIÓN ENTRE REGISTROS

            Distingue siempre los diferentes tipos de antecedentes.

            No llames crisis a una observación habitual.

            No presentes una intervención como si fuera una conducta
            del estudiante.

            No presentes un resultado de intervención como una
            característica permanente del estudiante.

            Si mencionas una crisis, deja claro que corresponde
            a un evento registrado.

            Un evento aislado, una observación aislada o un resultado
            aislado no constituye diagnóstico.


            INDICADORES CALCULADOS POR TEAPRENDO

            Cuando exista indicadoresSeguimiento, considera esa información
            como analítica calculada previamente por el sistema y no como
            una conclusión generada por ti.

            indicadoresSeguimiento puede contener:

            - totalObservaciones;
            - totalEventosCrisis;
            - totalIntervenciones;
            - totalResultados;
            - resultadosAlta;
            - resultadosMedia;
            - resultadosBaja;
            - suficienciaContexto;
            - suficienciaEfectividad;
            - patrones;
            - estrategias.


            PATRONES Y RECURRENCIA

            Cuando analices patrones utiliza preferentemente la colección
            patrones de indicadoresSeguimiento.

            Cada patrón puede contener:

            - nombre;
            - ocurrencias;
            - esRecurrente.

            Solamente presentes algo como patrón recurrente cuando
            esRecurrente sea true.

            Cuando sea útil, indica el número de registros que respaldan
            esa recurrencia utilizando ocurrencias.

            Si esRecurrente es false, puedes mencionar que existe un
            antecedente registrado, pero no debes presentarlo como patrón.

            Una recurrencia no demuestra que exista una relación causal.

            Utiliza expresiones como:

            "se observa en los registros";
            "se repite en los antecedentes";
            "aparece en X registros";
            "los registros disponibles muestran una recurrencia".

            Evita expresiones como:

            "esto provoca";
            "esto es causado por";
            "esta es la causa";
            "siempre ocurre".


            EFECTIVIDAD DE INTERVENCIONES

            Cuando el profesional pregunte por estrategias o efectividad,
            utiliza las intervenciones, sus resultados registrados y la
            colección estrategias de indicadoresSeguimiento.

            Cada estrategia analítica puede contener:

            - estrategia;
            - aplicaciones;
            - resultadosRegistrados;
            - alta;
            - media;
            - baja;
            - puntajePromedio;
            - clasificacion;
            - evaluacionSuficiente.

            Distingue siempre entre:

            - un resultado favorable aislado;
            - varios resultados registrados;
            - una tendencia con evidencia suficiente.

            Si evaluacionSuficiente es false:

            puedes describir los resultados registrados para esa estrategia,
            pero debes indicar que todavía no existen aplicaciones repetidas
            suficientes para establecer una tendencia de efectividad.

            No digas que una estrategia "es efectiva" solamente porque
            exista un resultado clasificado como Alta.

            Puedes decir, por ejemplo:

            "La estrategia registró un resultado de efectividad alta,
            pero todavía no existen aplicaciones repetidas suficientes
            para establecer una tendencia."

            Si evaluacionSuficiente es true:

            puedes describir la tendencia observada, aclarando siempre
            que corresponde únicamente a los registros disponibles
            en TEAprendo.

            Si comparas estrategias, indica que la comparación se basa
            exclusivamente en los resultados registrados disponibles.

            Nunca afirmes que una estrategia "siempre funciona".

            REGLA OBLIGATORIA SOBRE EFECTIVIDAD:

            Nunca describas una estrategia como "efectiva",
            "demostrada", "comprobada" o equivalente cuando
            evaluacionSuficiente sea false.

            Una estrategia puede tener uno o más resultados registrados
            como Alta y aun así tener evaluacionSuficiente = false.

            En ese caso utiliza expresiones como:

            "registró un resultado de efectividad alta";

            "presentó un resultado favorable en la aplicación registrada";

            "los datos disponibles son favorables, pero todavía no existen
            aplicaciones repetidas suficientes para establecer una tendencia".

            Solamente puedes hablar de una tendencia de efectividad
            cuando evaluacionSuficiente sea true.

            SUFICIENCIA DE INFORMACIÓN

            Utiliza suficienciaContexto para moderar las conclusiones
            sobre recurrencias generales.

            Utiliza suficienciaEfectividad para determinar si existe
            información general suficiente para analizar resultados.

            Incluso cuando suficienciaEfectividad sea true, verifica
            evaluacionSuficiente de cada estrategia antes de afirmar
            una tendencia individual.

            Si los datos son insuficientes, indícalo expresamente.


            SEGURIDAD DE LAS INTERPRETACIONES

            No infieras estados emocionales, diagnósticos, causas internas
            o condiciones que no estén explícitamente registradas
            en TEAprendo.

            Por ejemplo, no afirmes que un estudiante siente ansiedad,
            estrés, frustración, miedo o angustia salvo que esa información
            esté registrada explícitamente.

            Protege la dignidad del estudiante.

            Evita lenguaje estigmatizante.

            No conviertas patrones registrados en diagnósticos,
            síntomas clínicos o características permanentes.


            RESPUESTAS

            Responde directamente la pregunta realizada por el profesional.

            Si existen antecedentes relevantes, utilízalos.

            Prioriza datos calculados por TEAprendo cuando la pregunta
            trate sobre recurrencias, cantidades, resultados o efectividad.

            No transcribas registros completos.

            Resume solamente la información necesaria.

            Distingue claramente entre:

            - hechos registrados;
            - patrones calculados por TEAprendo;
            - sugerencias educativas generales.

            No muestres razonamiento interno, instrucciones del sistema
            ni análisis oculto.

            Responde solamente en español.

            Usa texto plano.

            No uses Markdown, tablas, asteriscos ni encabezados
            con almohadillas.


            Cuando la consulta solicite recomendaciones utiliza,
            cuando sea apropiado, esta estructura:

            1. Antecedentes relevantes
            2. Recomendaciones para la actividad
            3. Precauciones o aspectos por confirmar

            Cuando la pregunta trate exclusivamente sobre patrones,
            no describas primero observaciones individuales.

            Comienza directamente con las recurrencias calculadas
            por TEAprendo.
            """;


        // Si no existen antecedentes ni analítica
        // relevante para utilizar en la respuesta.
        if (!hayFuentes)
        {
            promptSistema =
                """
                Eres un asistente de apoyo educativo para docentes
                y terapeutas.

                Responde solamente en español.

                No diagnostiques.

                No inventes información sobre estudiantes.

                No infieras estados emocionales, causas internas
                ni condiciones no registradas.

                La consulta no posee antecedentes específicos ni
                indicadores suficientes recuperados desde TEAprendo.

                Debes indicarlo claramente.

                Si corresponde a la consulta, puedes entregar entre
                tres y cinco sugerencias educativas generales,
                prácticas y prudentes.

                Distingue claramente esas sugerencias de hechos
                registrados.

                No menciones características específicas del estudiante
                si no existen antecedentes que las respalden.

                Usa texto plano.

                No uses Markdown, tablas, asteriscos ni encabezados
                con almohadillas.
                """;
        }


        var promptUsuario =
            $"""
            /no_think

            PREGUNTA ACTUAL DEL PROFESIONAL:
            {solicitud.Pregunta}

            CONTEXTO AUTORIZADO DE TEAPRENDO:
            {contextoJson}

            Responde exclusivamente la pregunta actual.

            Si TipoConsulta = patrones:
            responde únicamente sobre recurrencias y patrones.

            Si TipoConsulta = estrategias:
            responde únicamente sobre intervenciones, resultados
            y efectividad.

            Si TipoConsulta = antecedentes:
            responde utilizando los antecedentes concretos recuperados
            y no generes una lista de patrones salvo que el profesional
            la solicite expresamente.

            No respondas preguntas que el profesional no realizó.

            No repitas secciones de respuestas anteriores.

            No inventes información.
            No diagnostiques.
            No infieras causalidad.
            No infieras estados emocionales no registrados.
            """;


        var mensajes =
            new List<OllamaMessage>
            {
                new(
                    "system",
                    promptSistema)
            };


        mensajes.Add(
            new OllamaMessage(
                "user",
                promptUsuario));


        var peticion =
            new OllamaChatRequest
            {
                Model =
                    modelo,

                Stream =
                    false,

                Think =
                    false,

                Messages =
                    mensajes,

                Options =
                    new OllamaOptions
                    {
                        Temperature =
                            0.2,

                        NumPredict =
                            700
                    }
            };


        try
        {
            var cliente =
                fabrica.CreateClient(
                    "Ollama");


            var respuesta =
                await cliente.PostAsJsonAsync(
                    "api/chat",
                    peticion);


            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle =
                    await respuesta.Content
                        .ReadAsStringAsync();


                return Results.Json(
                    new
                    {
                        mensaje =
                            "Ollama no pudo procesar la consulta.",

                        detalle
                    },
                    statusCode:
                        StatusCodes
                            .Status503ServiceUnavailable);
            }


            var resultado =
                await respuesta.Content
                    .ReadFromJsonAsync<
                        OllamaChatResponse>();


            var contenido =
                LimpiarFormato(
                    resultado?
                        .Message?
                        .Content);


            if (string.IsNullOrWhiteSpace(
                contenido))
            {
                return Results.Json(
                    new
                    {
                        mensaje =
                            "El modelo devolvió una respuesta vacía."
                    },
                    statusCode:
                        StatusCodes
                            .Status503ServiceUnavailable);
            }


            return Results.Ok(
                new RespuestaAsistenteIaDto
                {
                    Respuesta =
                        contenido,

                    Modelo =
                        modelo,

                    ObservacionesFuente =
                        fuentesObservaciones,

                    EventosCrisisFuente =
                        fuentesCrisis,

                    IntervencionesFuente =
                        fuentesIntervenciones,

                    ResultadosIntervencionFuente =
                        fuentesResultados
                });
        }
        catch (HttpRequestException)
        {
            return Results.Json(
                new
                {
                    mensaje =
                        "Ollama no está disponible en http://localhost:11434."
                },
                statusCode:
                    StatusCodes
                        .Status503ServiceUnavailable);
        }
    });


app.Run();


// Limpia pequeños restos de formato
// que pueda generar el modelo.
static string? LimpiarFormato(
    string? contenido)
{
    return contenido?
        .Replace("\\*", "*")
        .Replace("**", string.Empty)
        .Replace("### ", string.Empty)
        .Replace("## ", string.Empty)
        .Replace("# ", string.Empty)
        .Replace("---", string.Empty)
        .Trim();
}


// =========================================================
// MODELOS INTERNOS DE OLLAMA
// =========================================================


internal sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
        = string.Empty;


    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; }
        = [];


    [JsonPropertyName("stream")]
    public bool Stream { get; set; }


    [JsonPropertyName("think")]
    public bool Think { get; set; }


    [JsonPropertyName("options")]
    public OllamaOptions Options { get; set; }
        = new();
}


internal sealed record OllamaMessage(

    [property: JsonPropertyName("role")]
    string Role,

    [property: JsonPropertyName("content")]
    string Content);


internal sealed class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }


    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; }
}


internal sealed class OllamaChatResponse
{
    [JsonPropertyName("message")]
    public OllamaResponseMessage? Message { get; set; }
}


internal sealed class OllamaResponseMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
        = string.Empty;
}