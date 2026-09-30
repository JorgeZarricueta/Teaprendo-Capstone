namespace TEAprendo.Contracts.Administracion.Docentes;

// Aulas que quedarán asignadas al docente.
public class AsignarAulasDocenteRequest
{
    public List<Guid> AulaIds { get; set; }
        = new();
}