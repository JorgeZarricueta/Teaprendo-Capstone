namespace TEAprendo.Contracts.Profesional.Observaciones;

public class CrearObservacionRequest
{
    public Guid AlumnoId { get; set; }

    public Guid? ActividadId { get; set; }

    public DateTime FechaObservacion { get; set; }

    public string Contexto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Situacion { get; set; } = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoAplicado { get; set; }

    public string RespuestaAlumno { get; set; } = string.Empty;

    public bool? SeEstabilizo { get; set; }

    public string? Seguimiento { get; set; }
}
