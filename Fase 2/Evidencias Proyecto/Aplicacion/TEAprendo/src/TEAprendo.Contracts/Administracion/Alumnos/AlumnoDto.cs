namespace TEAprendo.Contracts.Administracion.Alumnos;

// Información administrativa de un alumno.
public class AlumnoDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public bool Activo { get; set; }

    public Guid AulaId { get; set; }

    public string AulaNombre { get; set; } = string.Empty;

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;
}