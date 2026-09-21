namespace TEAprendo.Contracts.Administracion.Directores;

// Información básica de una sede asignada.
public class SedeAsignadaDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string InstitucionNombre { get; set; } = string.Empty;
}