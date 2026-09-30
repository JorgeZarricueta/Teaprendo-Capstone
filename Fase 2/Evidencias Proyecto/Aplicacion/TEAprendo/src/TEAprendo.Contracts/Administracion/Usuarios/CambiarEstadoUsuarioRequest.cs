namespace TEAprendo.Contracts.Administracion.Usuarios;

// Permite activar o desactivar un usuario.
public class CambiarEstadoUsuarioRequest
{
    public bool Activo { get; set; }
}