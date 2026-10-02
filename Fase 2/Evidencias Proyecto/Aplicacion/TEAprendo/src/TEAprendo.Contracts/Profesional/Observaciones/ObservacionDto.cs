namespace TEAprendo.Contracts.Profesional.Observaciones;

public class ObservacionDto
{
    public Guid Id { get; set; }

    public Guid AlumnoId { get; set; }

    public Guid AutorUsuarioId { get; set; }

    public string AutorNombre { get; set; } = string.Empty;

    public string AutorRol { get; set; } = string.Empty;

    public Guid? ActividadId { get; set; }

    public string? ActividadTitulo { get; set; }

    public DateTime FechaObservacion { get; set; }

    public string Contexto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Situacion { get; set; } = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoAplicado { get; set; }

    public string RespuestaAlumno { get; set; } = string.Empty;

    public bool? SeEstabilizo { get; set; }

    public string? Seguimiento { get; set; }

    public DateTime FechaCreacionUtc { get; set; }

    public bool PuedeEditar { get; set; }
}
