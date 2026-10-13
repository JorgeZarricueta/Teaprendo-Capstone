namespace TEAprendo.Domain.Entities;

// Solicitud de cita entre un Apoderado y un profesional.
public class Appointment
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid GuardianUserId { get; set; }

    public Guid ProfessionalUserId { get; set; }

    public string ProfessionalRole { get; set; }
        = string.Empty;

    public string Reason { get; set; }
        = string.Empty;

    public DateTime RequestedStartDateTime { get; set; }

    public DateTime RequestedEndDateTime { get; set; }

    public string Status { get; set; }
        = "Pendiente";

    public string? ResponseMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RespondedAtUtc { get; set; }

    public Student Student { get; set; }
        = null!;
}