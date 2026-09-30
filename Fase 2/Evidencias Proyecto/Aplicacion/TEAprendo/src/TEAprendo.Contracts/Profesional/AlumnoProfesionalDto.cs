namespace TEAprendo.Contracts.Profesional;

// Alumno disponible para el profesional autenticado.
public class AlumnoProfesionalDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public Guid AulaId { get; set; }

    public string AulaNombre { get; set; } = string.Empty;

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;
}