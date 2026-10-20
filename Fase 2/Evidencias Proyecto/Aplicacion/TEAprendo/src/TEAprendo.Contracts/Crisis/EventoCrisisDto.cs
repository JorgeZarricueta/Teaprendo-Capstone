namespace TEAprendo.Contracts.Crisis;


// Evento de crisis devuelto al frontend.
public class EventoCrisisDto
{
    public Guid Id { get; set; }

    public Guid AlumnoId { get; set; }

    public string AlumnoNombre { get; set; }
        = string.Empty;

    public Guid CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; }
        = string.Empty;

    public DateTime FechaHoraEvento { get; set; }

    public int? DuracionMinutos { get; set; }

    public string Contexto { get; set; }
        = string.Empty;

    public string DescripcionEvento { get; set; }
        = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoInmediato { get; set; }

    public string? Resultado { get; set; }

    public bool? SeEstabilizo { get; set; }

    public bool RequirioApoyoAdicional { get; set; }

    public string? Notas { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}


// Datos necesarios para registrar
// rápidamente un evento desde el perfil.
public class CrearEventoCrisisRequest
{
    public Guid AlumnoId { get; set; }

    public DateTime FechaHoraEvento { get; set; }

    public int? DuracionMinutos { get; set; }

    public string Contexto { get; set; }
        = string.Empty;

    public string DescripcionEvento { get; set; }
        = string.Empty;

    public string? Desencadenante { get; set; }

    public string? ApoyoInmediato { get; set; }

    public string? Resultado { get; set; }

    public bool? SeEstabilizo { get; set; }

    public bool RequirioApoyoAdicional { get; set; }

    public string? Notas { get; set; }
}
