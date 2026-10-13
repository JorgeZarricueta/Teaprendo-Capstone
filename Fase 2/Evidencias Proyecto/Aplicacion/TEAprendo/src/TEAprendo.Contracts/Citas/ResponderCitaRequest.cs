namespace TEAprendo.Contracts.Citas;

// Respuesta del profesional a una solicitud.
public class ResponderCitaRequest
{
    public bool Aceptar { get; set; }

    public string? Mensaje { get; set; }
}