namespace TEAprendo.Domain.Entities;

public class Site
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InstitutionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Institution Institution { get; set; } = null!;

    public ICollection<Classroom> Classrooms { get; set; }
        = new List<Classroom>();
}