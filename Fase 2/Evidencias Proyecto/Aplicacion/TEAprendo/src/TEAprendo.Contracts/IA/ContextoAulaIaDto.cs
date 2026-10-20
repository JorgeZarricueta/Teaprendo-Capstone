namespace TEAprendo.Contracts.IA;


// Contexto autorizado que la API prepara para la IA.
public class ContextoAulaIaDto
{
    public Guid AulaId { get; set; }

    public string AulaNombre { get; set; }
        = string.Empty;

    public string SedeNombre { get; set; }
        = string.Empty;

    public string Alcance { get; set; }
        = string.Empty;

    public int TotalAlumnosConsiderados { get; set; }

    public int TotalObservacionesDisponibles { get; set; }

    public List<AlumnoContextoIaDto> Alumnos { get; set; }
        = [];
}


// Alumno incluido dentro del contexto autorizado.
public class AlumnoContextoIaDto
{
    public Guid AlumnoId { get; set; }

    public string Nombre { get; set; }
        = string.Empty;

    public List<ObservacionContextoIaDto> Observaciones { get; set; }
        = [];

    // Eventos de crisis relevantes del alumno.
    public List<EventoCrisisContextoIaDto> EventosCrisis { get; set; }
        = [];
}


// Observación conductual preparada para ser utilizada por la IA.
public class ObservacionContextoIaDto
{
    public Guid ObservacionId { get; set; }

    public DateTime Fecha { get; set; }

    public string Contexto { get; set; }
        = string.Empty;

    public string Conducta { get; set; }
        = string.Empty;

    public string? Antecedente { get; set; }

    public string? Consecuencia { get; set; }

    public string? Notas { get; set; }

    public int Relevancia { get; set; }
}

// Evento de crisis entregado como contexto a la IA.
public class EventoCrisisContextoIaDto
{
    public Guid EventoId { get; set; }

    public DateTime Fecha { get; set; }

    public string Contexto { get; set; }
        = string.Empty;

    public string Descripcion { get; set; }
        = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoInmediato { get; set; }

    public string? Resultado { get; set; }

    public bool? SeEstabilizo { get; set; }

    public bool RequirioApoyoAdicional { get; set; }

    public string? Notas { get; set; }

    public int Relevancia { get; set; }
}