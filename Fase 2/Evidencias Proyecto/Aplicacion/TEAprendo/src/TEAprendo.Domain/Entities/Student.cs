namespace TEAprendo.Domain.Entities;

// Representa a un alumno registrado en TEAprendo.
public class Student
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();

    // Actividades programadas para el alumno.
    public ICollection<Activity> Activities { get; set; }
        = new List<Activity>();
}