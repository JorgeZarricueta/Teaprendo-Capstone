namespace TEAprendo.Contracts.Administracion.Alumnos;

// Permite trasladar al alumno a otra aula.
public class CambiarAulaAlumnoRequest
{
    public Guid AulaId { get; set; }
}