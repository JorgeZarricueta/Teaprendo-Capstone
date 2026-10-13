namespace TEAprendo.Domain.Entities;

// Estrategia aplicada a partir de una observación.
public class Intervention
{
    public Guid Id { get; set; }

    public Guid BehavioralObservationId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Strategy { get; set; }
        = string.Empty;

    public string? Objective { get; set; }

    public DateTime InterventionDateTime { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public BehavioralObservation BehavioralObservation { get; set; }
        = null!;

    public ICollection<InterventionResult> Results { get; set; }
        = new List<InterventionResult>();
}