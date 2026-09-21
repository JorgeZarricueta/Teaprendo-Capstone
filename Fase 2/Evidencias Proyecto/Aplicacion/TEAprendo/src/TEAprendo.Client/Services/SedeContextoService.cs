using TEAprendo.Contracts.Administracion.Directores;

namespace TEAprendo.Client.Services;

// Mantiene la sede seleccionada durante la sesión.
public class SedeContextoService
{
    public SedeAsignadaDto? SedeActual { get; private set; }

    // Guarda la sede seleccionada.
    public void SeleccionarSede(SedeAsignadaDto sede)
    {
        SedeActual = sede;
    }

    // Limpia la sede actual.
    public void Limpiar()
    {
        SedeActual = null;
    }
}