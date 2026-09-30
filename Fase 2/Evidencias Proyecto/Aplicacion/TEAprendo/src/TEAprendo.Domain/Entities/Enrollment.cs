namespace TEAprendo.Domain.Entities;

// Relaciona un alumno con un aula.
public class Enrollment
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ClassroomId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public Student Student { get; set; } = null!;

    public Classroom Classroom { get; set; } = null!;
}