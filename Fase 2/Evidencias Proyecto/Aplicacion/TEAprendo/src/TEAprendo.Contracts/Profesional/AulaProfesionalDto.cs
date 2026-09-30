namespace TEAprendo.Contracts.Profesional;

// Aula asignada al docente autenticado.
public class AulaProfesionalDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;
}