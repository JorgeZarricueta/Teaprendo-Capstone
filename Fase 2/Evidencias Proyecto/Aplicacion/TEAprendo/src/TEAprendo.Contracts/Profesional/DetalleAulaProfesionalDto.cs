namespace TEAprendo.Contracts.Profesional;

// Detalle de un aula asignada al docente.
public class DetalleAulaProfesionalDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;

    public List<AlumnoProfesionalDto> Alumnos { get; set; }
        = new();
}