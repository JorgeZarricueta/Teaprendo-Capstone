namespace TEAprendo.Contracts.Seguimiento;

public class ObservacionConductualDto
{
    public Guid Id { get; set; }

    public Guid AlumnoId { get; set; }

    public string AlumnoNombre { get; set; }
        = string.Empty;

    public Guid CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; }
        = string.Empty;

    public DateTime FechaHoraObservacion { get; set; }

    public string Contexto { get; set; }
        = string.Empty;

    public string Conducta { get; set; }
        = string.Empty;

    public string? Antecedente { get; set; }

    public string? Consecuencia { get; set; }

    public string? Notas { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}