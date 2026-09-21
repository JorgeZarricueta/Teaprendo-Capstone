namespace TEAprendo.Contracts.Autenticacion;

// Datos necesarios para iniciar sesión.
public class LoginRequest
{
    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}