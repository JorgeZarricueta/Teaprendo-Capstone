namespace TEAprendo.Contracts.Administracion.Apoderados;

// Alumnos que quedarán vinculados al apoderado.
public class AsignarAlumnosApoderadoRequest
{
    public List<Guid> AlumnoIds { get; set; }
        = new();
}