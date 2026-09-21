using TEAprendo.Domain.Entities;

namespace TEAprendo.Infrastructure.Identity;

// Relaciona un usuario con una sede autorizada.
public class UsuarioSede
{
    public Guid UsuarioId { get; set; }

    public Guid SedeId { get; set; }

    public ApplicationUser Usuario { get; set; } = null!;

    public Site Sede { get; set; } = null!;
}