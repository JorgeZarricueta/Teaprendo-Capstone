namespace TEAprendo.Domain.Entities;

// Resultado registrado después de una intervención.
public class InterventionResult
{
    public Guid Id { get; set; }

    public Guid InterventionId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime ResultDateTime { get; set; }

    public string Outcome { get; set; }
        = string.Empty;

    public string Effectiveness { get; set; }
        = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Intervention Intervention { get; set; }
        = null!;
}