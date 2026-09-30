namespace TEAprendo.Contracts.Administracion.Alumnos;

// Datos personales modificables del alumno.
public class ActualizarAlumnoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }
}