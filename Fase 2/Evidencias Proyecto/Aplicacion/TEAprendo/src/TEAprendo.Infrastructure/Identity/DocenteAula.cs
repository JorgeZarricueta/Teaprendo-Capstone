using TEAprendo.Domain.Entities;

namespace TEAprendo.Infrastructure.Identity;

// Relaciona un docente con un aula asignada.
public class DocenteAula
{
    public Guid UsuarioId { get; set; }

    public Guid AulaId { get; set; }

    public ApplicationUser Usuario { get; set; } = null!;

    public Classroom Aula { get; set; } = null!;
}