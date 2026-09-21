namespace TEAprendo.Contracts.Administracion.Sedes;

// Información de una sede enviada al cliente.
public class SedeDto
{
    public Guid Id { get; set; }

    public Guid InstitucionId { get; set; }

    public string InstitucionNombre { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public bool Activa { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}