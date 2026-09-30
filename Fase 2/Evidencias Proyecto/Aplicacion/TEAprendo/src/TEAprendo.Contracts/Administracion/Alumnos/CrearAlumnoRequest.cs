namespace TEAprendo.Contracts.Administracion.Alumnos;

// Datos necesarios para registrar un alumno.
public class CrearAlumnoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public Guid AulaId { get; set; }
}