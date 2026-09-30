namespace TEAprendo.Contracts.Agenda;

// Datos necesarios para programar una actividad.
public class CrearActividadRequest
{
    public Guid AlumnoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }
}