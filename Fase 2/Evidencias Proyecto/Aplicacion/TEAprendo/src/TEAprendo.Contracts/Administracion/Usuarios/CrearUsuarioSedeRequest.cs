namespace TEAprendo.Contracts.Administracion.Usuarios;

// Datos necesarios para crear un usuario.
public class CrearUsuarioSedeRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public Guid SedeId { get; set; }
}