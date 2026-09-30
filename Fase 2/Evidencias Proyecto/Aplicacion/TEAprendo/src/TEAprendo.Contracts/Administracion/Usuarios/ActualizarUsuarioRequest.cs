namespace TEAprendo.Contracts.Administracion.Usuarios;

// Datos personales modificables del usuario.
public class ActualizarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;
}