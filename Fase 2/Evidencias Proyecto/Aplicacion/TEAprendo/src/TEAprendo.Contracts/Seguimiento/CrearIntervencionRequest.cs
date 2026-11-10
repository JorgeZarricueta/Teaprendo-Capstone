namespace TEAprendo.Contracts.Seguimiento;

public class CrearIntervencionRequest
{
    public Guid ObservacionId { get; set; }

    public DateTime FechaHoraIntervencion { get; set; }

    public string Estrategia { get; set; }
        = string.Empty;

    public string? Objetivo { get; set; }
}