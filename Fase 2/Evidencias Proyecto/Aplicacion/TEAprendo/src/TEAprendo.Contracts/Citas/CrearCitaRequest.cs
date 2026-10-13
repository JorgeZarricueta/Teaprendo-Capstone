namespace TEAprendo.Contracts.Citas;

// Solicitud enviada por un Apoderado.
public class CrearCitaRequest
{
    public Guid AlumnoId { get; set; }

    public Guid ProfesionalUsuarioId { get; set; }

    public string Motivo { get; set; }
        = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }
}