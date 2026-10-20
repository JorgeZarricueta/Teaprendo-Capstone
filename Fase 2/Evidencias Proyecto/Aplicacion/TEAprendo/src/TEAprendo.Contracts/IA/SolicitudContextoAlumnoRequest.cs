namespace TEAprendo.Contracts.IA;


// Solicitud del asistente IA enfocada en un solo alumno.
public class SolicitudContextoAlumnoRequest
{
    public Guid AlumnoId { get; set; }

    public string Pregunta { get; set; }
        = string.Empty;

    public int MaxObservaciones { get; set; }
        = 20;

    public List<MensajeHistorialIaDto> Historial { get; set; }
        = [];
}