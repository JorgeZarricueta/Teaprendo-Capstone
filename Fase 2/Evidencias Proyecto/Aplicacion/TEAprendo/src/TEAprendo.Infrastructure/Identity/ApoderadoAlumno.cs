using TEAprendo.Domain.Entities;

namespace TEAprendo.Infrastructure.Identity;

// Relaciona un apoderado con un alumno autorizado.
public class ApoderadoAlumno
{
    public Guid UsuarioId { get; set; }

    public Guid AlumnoId { get; set; }

    public ApplicationUser Usuario { get; set; } = null!;

    public Student Alumno { get; set; } = null!;
}