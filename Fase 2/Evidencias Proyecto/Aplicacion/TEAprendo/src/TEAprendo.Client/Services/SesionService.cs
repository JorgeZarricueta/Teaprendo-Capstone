using System.Net.Http.Json;
using TEAprendo.Contracts.Autenticacion;

namespace TEAprendo.Client.Services;

// Mantiene los datos del usuario autenticado.
public class SesionService
{
    private readonly HttpClient _http;

    public UsuarioActualDto? UsuarioActual { get; private set; }

    public SesionService(HttpClient http)
    {
        _http = http;
    }


    // Obtiene el usuario actual desde la API.
    public async Task<UsuarioActualDto?> CargarAsync()
    {
        try
        {
            UsuarioActual =
                await _http.GetFromJsonAsync<UsuarioActualDto>(
                    "api/autenticacion/usuario-actual");

            return UsuarioActual;
        }
        catch
        {
            UsuarioActual = null;

            return null;
        }
    }


    // Comprueba un rol del usuario actual.
    public bool TieneRol(string rol)
    {
        return UsuarioActual?.Roles.Any(x =>
            x.Equals(
                rol,
                StringComparison.OrdinalIgnoreCase))
            == true;
    }


    public void Limpiar()
    {
        UsuarioActual = null;
    }
}