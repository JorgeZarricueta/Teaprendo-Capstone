namespace TEAprendo.Contracts.Profesional;

// Resumen inicial para Docentes y Terapeutas.
public class ResumenProfesionalDto
{
    public string Rol { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public int CantidadAulas { get; set; }

    public int CantidadAlumnos { get; set; }

    public List<AulaProfesionalDto> Aulas { get; set; }
        = new();

    public List<AlumnoProfesionalDto> Alumnos { get; set; }
        = new();
}