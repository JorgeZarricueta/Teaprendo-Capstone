namespace TEAprendo.Contracts.Administracion.Aulas;

// Información de un aula enviada al cliente.
public class AulaDto
{
    public Guid Id { get; set; }

    public Guid SedeId { get; set; }

    public string SedeNombre { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }

    public bool Activa { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}