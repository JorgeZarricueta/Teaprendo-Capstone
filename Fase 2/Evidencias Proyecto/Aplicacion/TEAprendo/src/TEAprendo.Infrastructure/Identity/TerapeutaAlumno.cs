using TEAprendo.Domain.Entities;

namespace TEAprendo.Infrastructure.Identity;

// Relaciona un terapeuta con un alumno asignado.
public class TerapeutaAlumno
{
    public Guid UsuarioId { get; set; }

    public Guid AlumnoId { get; set; }

    public ApplicationUser Usuario { get; set; } = null!;

    public Student Alumno { get; set; } = null!;
}