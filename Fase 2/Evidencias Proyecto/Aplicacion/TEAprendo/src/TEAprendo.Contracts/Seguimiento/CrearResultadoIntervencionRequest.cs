namespace TEAprendo.Contracts.Seguimiento;

public class CrearResultadoIntervencionRequest
{
    public Guid IntervencionId { get; set; }

    public DateTime FechaHoraResultado { get; set; }

    public string Resultado { get; set; }
        = string.Empty;

    public string Efectividad { get; set; }
        = string.Empty;

    public string? Notas { get; set; }
}