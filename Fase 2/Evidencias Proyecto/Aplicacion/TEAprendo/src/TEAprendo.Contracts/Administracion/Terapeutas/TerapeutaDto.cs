namespace TEAprendo.Contracts.Administracion.Terapeutas;

// Información administrativa de un terapeuta.
public class TerapeutaDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public List<AlumnoAsignadoTerapeutaDto> Alumnos { get; set; }
        = new();
}