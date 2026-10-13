namespace TEAprendo.Domain.Entities;

// Registro conductual realizado por un profesional.
public class BehavioralObservation
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime ObservationDateTime { get; set; }

    public string Context { get; set; }
        = string.Empty;

    public string Behavior { get; set; }
        = string.Empty;

    public string? Antecedent { get; set; }

    public string? Consequence { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Student Student { get; set; }
        = null!;

    public ICollection<Intervention> Interventions { get; set; }
        = new List<Intervention>();
}