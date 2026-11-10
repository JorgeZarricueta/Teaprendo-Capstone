namespace TEAprendo.Contracts.Seguimiento;

public class ResultadoIntervencionDto
{
    public Guid Id { get; set; }

    public Guid IntervencionId { get; set; }

    public Guid CreadoPorUsuarioId { get; set; }

    public DateTime FechaHoraResultado { get; set; }

    public string Resultado { get; set; }
        = string.Empty;

    public string Efectividad { get; set; }
        = string.Empty;

    public string? Notas { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}