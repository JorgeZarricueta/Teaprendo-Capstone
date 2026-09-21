namespace TEAprendo.Contracts.Autenticacion;

// Información básica del usuario autenticado.
public class UsuarioActualDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}