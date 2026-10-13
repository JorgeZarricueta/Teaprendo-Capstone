namespace TEAprendo.Contracts.Citas;

// Información general de una solicitud de cita.
public class CitaDto
{
    public Guid Id { get; set; }

    public Guid AlumnoId { get; set; }

    public string AlumnoNombre { get; set; }
        = string.Empty;

    public Guid ApoderadoUsuarioId { get; set; }

    public string ApoderadoNombre { get; set; }
        = string.Empty;

    public Guid ProfesionalUsuarioId { get; set; }

    public string ProfesionalNombre { get; set; }
        = string.Empty;

    public string RolProfesional { get; set; }
        = string.Empty;

    public string Motivo { get; set; }
        = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public string Estado { get; set; }
        = string.Empty;

    public string? MensajeRespuesta { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}