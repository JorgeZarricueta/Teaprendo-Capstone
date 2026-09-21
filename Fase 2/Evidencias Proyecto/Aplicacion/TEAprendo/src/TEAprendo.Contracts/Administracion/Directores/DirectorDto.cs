namespace TEAprendo.Contracts.Administracion.Directores;

// Información administrativa de un Director.
public class DirectorDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public List<SedeAsignadaDto> Sedes { get; set; } = new();
}