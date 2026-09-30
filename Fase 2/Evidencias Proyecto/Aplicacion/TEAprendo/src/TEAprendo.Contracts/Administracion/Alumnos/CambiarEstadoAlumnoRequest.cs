namespace TEAprendo.Contracts.Administracion.Alumnos;

// Permite activar o desactivar un alumno.
public class CambiarEstadoAlumnoRequest
{
    public bool Activo { get; set; }
}