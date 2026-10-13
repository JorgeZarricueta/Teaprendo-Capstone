namespace TEAprendo.Contracts.Notificaciones;

public class NotificacionDto
{
    public Guid Id { get; set; }

    public string Tipo { get; set; }
        = string.Empty;

    public string Titulo { get; set; }
        = string.Empty;

    public string Mensaje { get; set; }
        = string.Empty;

    public string? Ruta { get; set; }

    public string? EntidadTipo { get; set; }

    public Guid? EntidadId { get; set; }

    public bool Leida { get; set; }

    public DateTime FechaCreacionUtc { get; set; }

    public DateTime? FechaLecturaUtc { get; set; }
}