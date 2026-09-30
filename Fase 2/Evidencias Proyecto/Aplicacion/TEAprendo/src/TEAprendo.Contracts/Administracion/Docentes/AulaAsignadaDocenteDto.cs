namespace TEAprendo.Contracts.Administracion.Docentes;

// Aula asignada a un docente.
public class AulaAsignadaDocenteDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }
}