namespace TEAprendo.Domain.Entities;

public class Classroom
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SiteId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Site Site { get; set; } = null!;
}