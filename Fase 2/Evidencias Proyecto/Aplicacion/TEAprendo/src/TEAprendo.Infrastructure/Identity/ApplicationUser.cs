using Microsoft.AspNetCore.Identity;

namespace TEAprendo.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}