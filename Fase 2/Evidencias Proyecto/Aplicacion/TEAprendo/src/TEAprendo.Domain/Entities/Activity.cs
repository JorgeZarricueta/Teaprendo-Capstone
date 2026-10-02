namespace TEAprendo.Domain.Entities;

// Representa una actividad programada para un alumno.
public class Activity
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public Student Student { get; set; } = null!;

    public ICollection<StudentObservation> Observations { get; set; }
        = new List<StudentObservation>();
}
