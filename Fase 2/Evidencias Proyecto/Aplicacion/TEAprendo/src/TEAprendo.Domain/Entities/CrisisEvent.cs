namespace TEAprendo.Domain.Entities;


// Registro breve de una situación de crisis
// ocurrida durante la jornada del estudiante.
public class CrisisEvent
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid CreatedByUserId { get; set; }


    // Fecha y hora local en que ocurrió el evento.
    public DateTime EventDateTime { get; set; }


    // Duración aproximada si el profesional la conoce.
    public int? DurationMinutes { get; set; }


    public string Context { get; set; }
        = string.Empty;


    // Descripción breve de lo ocurrido.
    public string EventDescription { get; set; }
        = string.Empty;


    // Posible desencadenante observado.
    public string? Trigger { get; set; }


    // Apoyo aplicado inmediatamente.
    public string? ImmediateSupport { get; set; }


    // Estado posterior o resultado del apoyo.
    public string? Outcome { get; set; }


    public bool? WasStabilized { get; set; }


    public bool RequiredAdditionalSupport { get; set; }


    public string? Notes { get; set; }


    // Fecha técnica de creación.
    public DateTime CreatedAtUtc { get; set; }


    public Student Student { get; set; }
        = null!;
}