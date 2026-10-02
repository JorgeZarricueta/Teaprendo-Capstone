namespace TEAprendo.Contracts.IA;

public class SolicitudContextoAulaRequest
{
    public Guid AulaId { get; set; }

    public string Pregunta { get; set; } = string.Empty;

    public int MaxObservaciones { get; set; } = 20;

    public List<MensajeHistorialIaDto> Historial { get; set; } = [];
}

public class MensajeHistorialIaDto
{
    public bool EsUsuario { get; set; }

    public string Texto { get; set; } = string.Empty;
}
