namespace TEAprendo.Contracts.Administracion.Terapeutas;

// Alumnos que quedarán asignados al terapeuta.
public class AsignarAlumnosTerapeutaRequest
{
    public List<Guid> AlumnoIds { get; set; }
        = new();
}