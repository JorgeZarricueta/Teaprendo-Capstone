namespace TEAprendo.Contracts.Administracion.Docentes;

// Información administrativa de un docente.
public class DocenteDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public List<AulaAsignadaDocenteDto> Aulas { get; set; }
        = new();
}