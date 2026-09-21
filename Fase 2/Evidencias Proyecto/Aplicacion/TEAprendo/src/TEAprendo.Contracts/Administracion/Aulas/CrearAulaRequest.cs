namespace TEAprendo.Contracts.Administracion.Aulas;

// Datos necesarios para crear un aula.
public class CrearAulaRequest
{
    public Guid SedeId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }
}