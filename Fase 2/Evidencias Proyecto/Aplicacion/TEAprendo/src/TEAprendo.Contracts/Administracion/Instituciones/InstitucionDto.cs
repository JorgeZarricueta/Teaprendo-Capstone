namespace TEAprendo.Contracts.Administracion.Instituciones;

// Información de una institución enviada al cliente.
public class InstitucionDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}