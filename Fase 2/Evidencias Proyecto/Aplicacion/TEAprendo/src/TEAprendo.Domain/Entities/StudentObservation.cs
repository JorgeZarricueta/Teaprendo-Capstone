namespace TEAprendo.Domain.Entities;

public class StudentObservation
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid AuthorUserId { get; set; }

    public Guid? ActivityId { get; set; }

    public DateTime ObservedAt { get; set; }

    public string Context { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Situation { get; set; } = string.Empty;

    public string? Trigger { get; set; }

    public string? SupportApplied { get; set; }

    public string StudentResponse { get; set; } = string.Empty;

    public bool? WasStabilized { get; set; }

    public string? FollowUp { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public Student Student { get; set; } = null!;

    public Activity? Activity { get; set; }
}
