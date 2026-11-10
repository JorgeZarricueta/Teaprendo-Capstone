namespace TEAprendo.Contracts.Seguimiento;

public class IntervencionDto
{
    public Guid Id { get; set; }

    public Guid ObservacionId { get; set; }

    public Guid AlumnoId { get; set; }

    public Guid CreadoPorUsuarioId { get; set; }

    public DateTime FechaHoraIntervencion { get; set; }

    public string Estrategia { get; set; }
        = string.Empty;

    public string? Objetivo { get; set; }

    public DateTime FechaCreacionUtc { get; set; }

    public List<ResultadoIntervencionDto> Resultados { get; set; }
        = [];
}