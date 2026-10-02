using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEAprendo.Contracts.IA;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Ollama:Url"] ??
        "http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { estado = "disponible" }));

app.MapPost(
    "/api/inferencia/responder",
    async (
        SolicitudInferenciaIaDto solicitud,
        IHttpClientFactory fabrica,
        IConfiguration configuracion) =>
    {
        if (string.IsNullOrWhiteSpace(solicitud.Pregunta) ||
            solicitud.Contexto.Alumnos.Count == 0)
        {
            return Results.BadRequest(new
            {
                mensaje = "La pregunta y el contexto son obligatorios."
            });
        }

        var modelo = configuracion["Ollama:Modelo"] ?? "qwen3:4b";
        var fuentes = solicitud.Contexto.Alumnos
            .SelectMany(x => x.Observaciones)
            .Select(x => x.ObservacionId)
            .Distinct()
            .ToList();

        var contextoParaModelo = new
        {
            solicitud.Contexto.AulaNombre,
            solicitud.Contexto.SedeNombre,
            solicitud.Contexto.Alcance,
            solicitud.Contexto.TotalAlumnosConsiderados,
            Alumnos = solicitud.Contexto.Alumnos
                .Where(x => x.Observaciones.Count > 0)
                .OrderByDescending(x => x.Observaciones.Max(o => o.Relevancia))
        };

        var contextoJson = JsonSerializer.Serialize(
            contextoParaModelo,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

        var promptSistema = """
            Eres un asistente de apoyo educativo para docentes y terapeutas.
            No diagnostiques, no atribuyas trastornos y no sustituyas decisiones profesionales.
            Para afirmar hechos sobre estudiantes usa exclusivamente los antecedentes
            proporcionados. Nunca inventes datos personales ni reacciones no registradas.
            Puedes entregar sugerencias educativas generales cuando falten antecedentes,
            identificándolas expresamente como orientación general y no como hechos del aula.
            Distingue claramente entre hechos registrados y sugerencias.
            No infieras una reacción a un color, material, sonido o actividad específicos
            a partir de una categoría más amplia. Indica qué detalle no está confirmado.
            Menciona solo a los alumnos para quienes exista información pertinente.
            En recomendaciones grupales identifica por nombre a quienes requieran una
            precaución concreta, priorizando antecedentes recientes y de mayor relevancia.
            Protege la dignidad del estudiante y evita lenguaje estigmatizante.
            No muestres razonamiento interno, análisis ni instrucciones recibidas.
            Entrega directamente la respuesta final y escribe solamente en español.
            Usa texto plano. No uses Markdown, asteriscos, almohadillas ni tablas.
            Si la pregunta solicita un dato básico disponible en el contexto, responde
            directamente en una o dos frases, sin agregar recomendaciones innecesarias.
            Si se solicita orientación y no hay observaciones relacionadas, entrega
            entre tres y cinco sugerencias educativas generales aplicables y declara
            claramente que no existen antecedentes específicos recuperados para esa consulta.
            No respondas solamente que faltan antecedentes.
            Para recomendaciones basadas en antecedentes usa esta estructura breve:
            1. Antecedentes relevantes
            2. Recomendaciones para la actividad
            3. Precauciones y aspectos por confirmar
            Sintetiza los antecedentes; no transcribas cada ficha ni enumeres cada campo.
            Responde primero la pregunta concreta del profesional.
            Si los antecedentes no bastan, dilo explícitamente.
            """;

        if (fuentes.Count == 0)
        {
            promptSistema = """
                Eres un asistente de apoyo educativo para docentes y terapeutas.
                Responde solamente en español. No diagnostiques ni inventes información
                sobre estudiantes. La consulta no tiene antecedentes específicos asociados.
                Debes indicarlo en una frase y luego proponer entre tres y cinco sugerencias
                educativas generales, prácticas y prudentes que respondan a la actividad.
                Incluye precauciones sensoriales y de seguridad pertinentes.
                No menciones nombres. No respondas únicamente que faltan antecedentes.
                Usa texto plano. No uses Markdown, asteriscos, almohadillas ni tablas.
                """;
        }

        var promptUsuario = $"""
            /no_think

            Pregunta del profesional:
            {solicitud.Pregunta}

            Contexto autorizado recuperado desde TEAprendo:
            {contextoJson}

            Instrucción final: responde la pregunta del profesional con una síntesis breve.
            No transcribas las observaciones completas ni hagas una ficha por estudiante.
            """;

        var mensajes = new List<OllamaMessage>
        {
            new("system", promptSistema)
        };
        mensajes.AddRange(solicitud.Historial.Select(x =>
            new OllamaMessage(x.EsUsuario ? "user" : "assistant", x.Texto)));
        mensajes.Add(new OllamaMessage("user", promptUsuario));

        var peticion = new OllamaChatRequest
        {
            Model = modelo,
            Stream = false,
            Think = false,
            Messages = mensajes,
            Options = new OllamaOptions
            {
                Temperature = 0.2,
                NumPredict = 700
            }
        };

        try
        {
            var cliente = fabrica.CreateClient("Ollama");
            var respuesta = await cliente.PostAsJsonAsync("api/chat", peticion);

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();
                return Results.Json(
                    new
                    {
                        mensaje = "Ollama no pudo procesar la consulta.",
                        detalle
                    },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var resultado = await respuesta.Content
                .ReadFromJsonAsync<OllamaChatResponse>();
            var contenido = LimpiarFormato(resultado?.Message?.Content);

            if (string.IsNullOrWhiteSpace(contenido))
            {
                return Results.Json(
                    new { mensaje = "El modelo devolvió una respuesta vacía." },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            return Results.Ok(new RespuestaAsistenteIaDto
            {
                Respuesta = contenido,
                Modelo = modelo,
                ObservacionesFuente = fuentes
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
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    });

app.Run();

static string? LimpiarFormato(string? contenido)
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

internal sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("think")]
    public bool Think { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions Options { get; set; } = new();
}

internal sealed record OllamaMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

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
    public string Content { get; set; } = string.Empty;
}
