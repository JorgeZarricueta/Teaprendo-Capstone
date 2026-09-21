namespace TEAprendo.Contracts.Administracion.Aulas;

// Datos modificables de un aula.
public class ActualizarAulaRequest
{
    public string Nombre { get; set; } = string.Empty;

    public int AnioAcademico { get; set; }
}