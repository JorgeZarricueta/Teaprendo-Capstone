namespace TEAprendo.Contracts.Administracion.Apoderados;

// Alumno asociado a un apoderado.
public class AlumnoAsignadoApoderadoDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string AulaNombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}