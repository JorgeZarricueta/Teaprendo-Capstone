namespace TEAprendo.Contracts.IA;


// Información que TEAprendo.Api enviará a TEAprendo.AI.
public class SolicitudInferenciaIaDto
{
    public string Pregunta { get; set; }
        = string.Empty;

    public ContextoAulaIaDto Contexto { get; set; }
        = new();

    public List<MensajeHistorialIaDto> Historial { get; set; }
        = [];
}


// Respuesta que TEAprendo.AI devuelve al sistema.
public class RespuestaAsistenteIaDto
{
    public string Respuesta { get; set; }
        = string.Empty;

    public string Modelo { get; set; }
        = string.Empty;

    public List<Guid> ObservacionesFuente { get; set; }
        = [];

    public string Advertencia { get; set; }
        =
            "Orientación educativa basada en antecedentes registrados; no constituye diagnóstico.";
}