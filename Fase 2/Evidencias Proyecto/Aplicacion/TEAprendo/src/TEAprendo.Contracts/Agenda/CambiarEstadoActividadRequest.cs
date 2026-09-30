namespace TEAprendo.Contracts.Agenda;

// Permite activar o cancelar una actividad.
public class CambiarEstadoActividadRequest
{
    public bool Activa { get; set; }
}