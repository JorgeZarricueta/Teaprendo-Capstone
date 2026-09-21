namespace TEAprendo.Contracts.Administracion.Sedes;

// Datos modificables de una sede.
public class ActualizarSedeRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }
}