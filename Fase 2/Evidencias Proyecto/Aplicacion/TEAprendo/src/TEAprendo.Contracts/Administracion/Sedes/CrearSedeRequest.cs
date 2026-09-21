namespace TEAprendo.Contracts.Administracion.Sedes;

// Datos necesarios para crear una sede.
public class CrearSedeRequest
{
    public Guid InstitucionId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }
}
