namespace TEAprendo.Contracts.IA;

public class ContextoAulaIaDto
{
    public Guid AulaId { get; set; }

    public string AulaNombre { get; set; } = string.Empty;

    public string SedeNombre { get; set; } = string.Empty;

    public string Alcance { get; set; } = string.Empty;

    public int TotalAlumnosConsiderados { get; set; }

    public int TotalObservacionesDisponibles { get; set; }

    public List<AlumnoContextoIaDto> Alumnos { get; set; } = [];
}

public class AlumnoContextoIaDto
{
    public Guid AlumnoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public List<ObservacionContextoIaDto> Observaciones { get; set; } = [];
}

public class ObservacionContextoIaDto
{
    public Guid ObservacionId { get; set; }

    public DateTime Fecha { get; set; }

    public string Contexto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Situacion { get; set; } = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoAplicado { get; set; }

    public string RespuestaAlumno { get; set; } = string.Empty;

    public bool? SeEstabilizo { get; set; }

    public string? Seguimiento { get; set; }

    public int Relevancia { get; set; }
}
