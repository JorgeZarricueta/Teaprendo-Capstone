namespace TEAprendo.Contracts.Agenda;

// Datos modificables de una actividad.
public class ActualizarActividadRequest
{
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }
}