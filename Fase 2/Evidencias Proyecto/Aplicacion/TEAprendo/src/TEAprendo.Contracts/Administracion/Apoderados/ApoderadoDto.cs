namespace TEAprendo.Contracts.Administracion.Apoderados;

// Información administrativa de un apoderado.
public class ApoderadoDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public List<AlumnoAsignadoApoderadoDto> Alumnos { get; set; }
        = new();
}