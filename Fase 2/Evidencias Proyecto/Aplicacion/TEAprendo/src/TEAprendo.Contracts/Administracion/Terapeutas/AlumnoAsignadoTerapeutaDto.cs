namespace TEAprendo.Contracts.Administracion.Terapeutas;

// Alumno asignado a un terapeuta.
public class AlumnoAsignadoTerapeutaDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string AulaNombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}