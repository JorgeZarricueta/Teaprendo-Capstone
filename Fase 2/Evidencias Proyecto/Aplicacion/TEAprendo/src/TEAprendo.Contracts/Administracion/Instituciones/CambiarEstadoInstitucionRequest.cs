namespace TEAprendo.Contracts.Administracion.Instituciones;

// Permite activar o desactivar una institución.
public class CambiarEstadoInstitucionRequest
{
    public bool Activa { get; set; }
}