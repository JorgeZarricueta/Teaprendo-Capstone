namespace TEAprendo.Contracts.Citas;

// Profesional disponible para solicitar una cita.
public class ProfesionalCitaDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; }
        = string.Empty;

    public string Apellido { get; set; }
        = string.Empty;

    public string Rol { get; set; }
        = string.Empty;
}