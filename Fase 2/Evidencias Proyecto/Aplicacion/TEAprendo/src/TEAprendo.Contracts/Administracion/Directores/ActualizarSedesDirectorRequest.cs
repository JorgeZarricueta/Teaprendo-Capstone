namespace TEAprendo.Contracts.Administracion.Directores;

// Actualiza las sedes administradas por un Director.
public class ActualizarSedesDirectorRequest
{
    public List<Guid> SedeIds { get; set; } = new();
}