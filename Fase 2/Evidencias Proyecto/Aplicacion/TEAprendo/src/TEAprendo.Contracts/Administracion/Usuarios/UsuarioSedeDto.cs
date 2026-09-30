namespace TEAprendo.Contracts.Administracion.Usuarios;

// Información de un usuario dentro de una sede.
public class UsuarioSedeDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;
}