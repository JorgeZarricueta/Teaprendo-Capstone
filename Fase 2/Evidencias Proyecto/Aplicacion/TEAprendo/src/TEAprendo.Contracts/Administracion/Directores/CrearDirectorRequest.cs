namespace TEAprendo.Contracts.Administracion.Directores;

// Datos necesarios para crear un Director.
public class CrearDirectorRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public List<Guid> SedeIds { get; set; } = new();
}