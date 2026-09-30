namespace TEAprendo.Contracts.Agenda;

// Actividad visible en la agenda.
public class ActividadDto
{
    public Guid Id { get; set; }

    public Guid AlumnoId { get; set; }

    public string AlumnoNombre { get; set; } = string.Empty;

    public string AulaNombre { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }

    public bool Activa { get; set; }
}