namespace TEAprendo.Contracts.Citas;

// Alumno disponible para solicitar una cita.
public class AlumnoCitaDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; }
        = string.Empty;

    public string Apellido { get; set; }
        = string.Empty;

    public string AulaNombre { get; set; }
        = string.Empty;
}