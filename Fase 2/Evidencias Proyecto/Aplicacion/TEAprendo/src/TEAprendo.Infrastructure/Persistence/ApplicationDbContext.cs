using Microsoft.EntityFrameworkCore;
using TEAprendo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TEAprendo.Infrastructure.Identity;

namespace TEAprendo.Infrastructure.Persistence;

public class ApplicationDbContext :
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Relación entre usuarios y sedes autorizadas.
    public DbSet<Institution> Institutions => Set<Institution>();

    public DbSet<Site> Sites => Set<Site>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<UsuarioSede> UsuariosSedes => Set<UsuarioSede>();

    public DbSet<DocenteAula> DocentesAulas => Set<DocenteAula>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<ApoderadoAlumno> ApoderadosAlumnos
    => Set<ApoderadoAlumno>();

    public DbSet<TerapeutaAlumno> TerapeutasAlumnos
    => Set<TerapeutaAlumno>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<Appointment> Appointments =>
    Set<Appointment>();
    
    public DbSet<Notification> Notifications =>
    Set<Notification>();

    public DbSet<BehavioralObservation> BehavioralObservations =>
    Set<BehavioralObservation>();

    public DbSet<Intervention> Interventions =>
        Set<Intervention>();

    public DbSet<InterventionResult> InterventionResults =>
        Set<InterventionResult>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =========================
        // Identity
        // =========================

        modelBuilder.Entity<ApplicationUser>()
            .ToTable("usuarios");

        modelBuilder.Entity<IdentityRole<Guid>>()
            .ToTable("roles");

        modelBuilder.Entity<IdentityUserRole<Guid>>()
            .ToTable("usuarios_roles");

        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("usuarios_atributos");

        modelBuilder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("roles_atributos");

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("usuarios_accesos");

        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .ToTable("usuarios_tokens");


        // =========================
        // Instituciones
        // =========================

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.ToTable("instituciones");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Name)
                .HasColumnName("nombre")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                .HasColumnName("activa");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc");
        });


        // =========================
        // Sedes
        // =========================

        modelBuilder.Entity<Site>(entity =>
        {
            entity.ToTable("sedes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.InstitutionId)
                .HasColumnName("institucion_id");

            entity.Property(x => x.Name)
                .HasColumnName("nombre")
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Address)
                .HasColumnName("direccion");

            entity.Property(x => x.IsActive)
                .HasColumnName("activa");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc");

            // Relación sede → institución.
            entity.HasOne(x => x.Institution)
                .WithMany(x => x.Sites)
                .HasForeignKey(x => x.InstitutionId);
        });


        // =========================
        // Aulas
        // =========================

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.ToTable("aulas");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.SiteId)
                .HasColumnName("sede_id");

            entity.Property(x => x.Name)
                .HasColumnName("nombre")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.AcademicYear)
                .HasColumnName("anio_academico");

            entity.Property(x => x.IsActive)
                .HasColumnName("activa");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc");

            // Relación aula → sede.
            entity.HasOne(x => x.Site)
                .WithMany(x => x.Classrooms)
                .HasForeignKey(x => x.SiteId);
        });

        // =========================
        // Usuarios y sedes
        // =========================

        modelBuilder.Entity<UsuarioSede>(entity =>
        {
            entity.ToTable("usuarios_sedes");

            // Evita asignaciones duplicadas.
            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.SedeId
            });

            entity.Property(x => x.UsuarioId)
                .HasColumnName("usuario_id");

            entity.Property(x => x.SedeId)
                .HasColumnName("sede_id");

            // Relación con el usuario.
            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con la sede.
            entity.HasOne(x => x.Sede)
                .WithMany()
                .HasForeignKey(x => x.SedeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Docentes - Aulas
        // =========================

        modelBuilder.Entity<DocenteAula>(entity =>
        {
            entity.ToTable("docentes_aulas");

            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.AulaId
            });

            entity.Property(x => x.UsuarioId)
                .HasColumnName("usuario_id");

            entity.Property(x => x.AulaId)
                .HasColumnName("aula_id");

            // Relación con el usuario Docente.
            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con el aula.
            entity.HasOne(x => x.Aula)
                .WithMany()
                .HasForeignKey(x => x.AulaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Alumnos
        // =========================

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("alumnos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.FirstName)
                .HasColumnName("nombre")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .HasColumnName("apellido")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.BirthDate)
                .HasColumnName("fecha_nacimiento")
                .HasColumnType("date");

            entity.Property(x => x.IsActive)
                .HasColumnName("activo");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc");
        });


        // =========================
        // Matrículas
        // =========================

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("matriculas");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.StudentId)
                .HasColumnName("alumno_id");

            entity.Property(x => x.ClassroomId)
                .HasColumnName("aula_id");

            entity.Property(x => x.StartDate)
                .HasColumnName("fecha_inicio")
                .HasColumnType("date");

            entity.Property(x => x.EndDate)
                .HasColumnName("fecha_fin")
                .HasColumnType("date");

            entity.Property(x => x.IsActive)
                .HasColumnName("activa");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc");

            // Relación matrícula → alumno.
            entity.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId);

            // Relación matrícula → aula.
            entity.HasOne(x => x.Classroom)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.ClassroomId);
        });

        // =========================
        // Apoderados - Alumnos
        // =========================

        modelBuilder.Entity<ApoderadoAlumno>(entity =>
        {
            entity.ToTable("apoderados_alumnos");

            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.AlumnoId
            });

            entity.Property(x => x.UsuarioId)
                .HasColumnName("usuario_id");

            entity.Property(x => x.AlumnoId)
                .HasColumnName("alumno_id");

            // Relación con el usuario Apoderado.
            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con el alumno.
            entity.HasOne(x => x.Alumno)
                .WithMany()
                .HasForeignKey(x => x.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Terapeutas - Alumnos
        // =========================

        modelBuilder.Entity<TerapeutaAlumno>(entity =>
        {
            entity.ToTable("terapeutas_alumnos");

            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.AlumnoId
            });

            entity.Property(x => x.UsuarioId)
                .HasColumnName("usuario_id");

            entity.Property(x => x.AlumnoId)
                .HasColumnName("alumno_id");

            // Relación con el usuario Terapeuta.
            entity.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con el alumno.
            entity.HasOne(x => x.Alumno)
                .WithMany()
                .HasForeignKey(x => x.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Actividades
        // =========================

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("actividades");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.StudentId)
                .HasColumnName("alumno_id");

            entity.Property(x => x.CreatedByUserId)
                .HasColumnName("creado_por_usuario_id");

            entity.Property(x => x.Title)
                .HasColumnName("titulo")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnName("descripcion")
                .HasMaxLength(1000);

            entity.Property(x => x.Type)
                .HasColumnName("tipo")
                .HasMaxLength(50)
                .IsRequired();

            // Fecha y hora local programada para la actividad.
            entity.Property(x => x.StartDateTime)
                .HasColumnName("fecha_hora_inicio")
                .HasColumnType("timestamp without time zone");

            entity.Property(x => x.EndDateTime)
                .HasColumnName("fecha_hora_fin")
                .HasColumnType("timestamp without time zone");

            entity.Property(x => x.IsActive)
                .HasColumnName("activa");

            // Fecha técnica de creación almacenada en UTC.
            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType("timestamp with time zone");

            // Relación con el alumno.
            entity.HasOne(x => x.Student)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Citas
        // =========================

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("citas");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.Id)
                .HasColumnName("id");


            entity.Property(x => x.StudentId)
                .HasColumnName("alumno_id");


            entity.Property(x => x.GuardianUserId)
                .HasColumnName("apoderado_usuario_id");


            entity.Property(x => x.ProfessionalUserId)
                .HasColumnName("profesional_usuario_id");


            entity.Property(x => x.ProfessionalRole)
                .HasColumnName("rol_profesional")
                .HasMaxLength(30)
                .IsRequired();


            entity.Property(x => x.Reason)
                .HasColumnName("motivo")
                .HasMaxLength(1000)
                .IsRequired();


            // Horarios locales propuestos.
            entity.Property(x => x.RequestedStartDateTime)
                .HasColumnName(
                    "fecha_hora_inicio_solicitada")
                .HasColumnType(
                    "timestamp without time zone");


            entity.Property(x => x.RequestedEndDateTime)
                .HasColumnName(
                    "fecha_hora_fin_solicitada")
                .HasColumnType(
                    "timestamp without time zone");


            entity.Property(x => x.Status)
                .HasColumnName("estado")
                .HasMaxLength(30)
                .IsRequired();


            entity.Property(x => x.ResponseMessage)
                .HasColumnName("mensaje_respuesta")
                .HasMaxLength(1000);


            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType(
                    "timestamp with time zone");


            entity.Property(x => x.RespondedAtUtc)
                .HasColumnName("fecha_respuesta_utc")
                .HasColumnType(
                    "timestamp with time zone");


            // Una cita pertenece a un alumno.
            entity.HasOne(x => x.Student)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Notificaciones
        // =========================

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notificaciones");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.Id)
                .HasColumnName("id");


            entity.Property(x => x.UserId)
                .HasColumnName("usuario_id")
                .IsRequired();


            entity.Property(x => x.Type)
                .HasColumnName("tipo")
                .HasMaxLength(50)
                .IsRequired();


            entity.Property(x => x.Title)
                .HasColumnName("titulo")
                .HasMaxLength(150)
                .IsRequired();


            entity.Property(x => x.Message)
                .HasColumnName("mensaje")
                .HasMaxLength(1000)
                .IsRequired();


            entity.Property(x => x.Route)
                .HasColumnName("ruta")
                .HasMaxLength(250);


            entity.Property(x => x.EntityType)
                .HasColumnName("entidad_tipo")
                .HasMaxLength(50);


            entity.Property(x => x.EntityId)
                .HasColumnName("entidad_id");


            entity.Property(x => x.IsRead)
                .HasColumnName("leida")
                .HasDefaultValue(false)
                .IsRequired();


            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType(
                    "timestamp with time zone")
                .IsRequired();


            entity.Property(x => x.ReadAtUtc)
                .HasColumnName("fecha_lectura_utc")
                .HasColumnType(
                    "timestamp with time zone");


            // La notificación pertenece a un usuario.
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // Observaciones conductuales
        // =========================

        modelBuilder.Entity<BehavioralObservation>(entity =>
        {
            entity.ToTable("observaciones_conductuales");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.Id)
                .HasColumnName("id");


            entity.Property(x => x.StudentId)
                .HasColumnName("alumno_id")
                .IsRequired();


            entity.Property(x => x.CreatedByUserId)
                .HasColumnName("creado_por_usuario_id")
                .IsRequired();


            entity.Property(x => x.ObservationDateTime)
                .HasColumnName("fecha_hora_observacion")
                .HasColumnType(
                    "timestamp without time zone")
                .IsRequired();


            entity.Property(x => x.Context)
                .HasColumnName("contexto")
                .HasMaxLength(1000)
                .IsRequired();


            entity.Property(x => x.Behavior)
                .HasColumnName("conducta")
                .HasMaxLength(1500)
                .IsRequired();


            entity.Property(x => x.Antecedent)
                .HasColumnName("antecedente")
                .HasMaxLength(1000);


            entity.Property(x => x.Consequence)
                .HasColumnName("consecuencia")
                .HasMaxLength(1000);


            entity.Property(x => x.Notes)
                .HasColumnName("notas")
                .HasMaxLength(1500);


            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType(
                    "timestamp with time zone")
                .IsRequired();


            entity.HasOne(x => x.Student)
                .WithMany(x =>
                    x.BehavioralObservations)
                .HasForeignKey(x =>
                    x.StudentId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x =>
                    x.CreatedByUserId)
                .OnDelete(
                    DeleteBehavior.Restrict);
        });

        // =========================
        // Intervenciones
        // =========================

        modelBuilder.Entity<Intervention>(entity =>
        {
            entity.ToTable("intervenciones");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.Id)
                .HasColumnName("id");


            entity.Property(x => x.BehavioralObservationId)
                .HasColumnName("observacion_id")
                .IsRequired();


            entity.Property(x => x.CreatedByUserId)
                .HasColumnName("creado_por_usuario_id")
                .IsRequired();


            entity.Property(x => x.Strategy)
                .HasColumnName("estrategia")
                .HasMaxLength(1500)
                .IsRequired();


            entity.Property(x => x.Objective)
                .HasColumnName("objetivo")
                .HasMaxLength(1000);


            entity.Property(x => x.InterventionDateTime)
                .HasColumnName("fecha_hora_intervencion")
                .HasColumnType(
                    "timestamp without time zone")
                .IsRequired();


            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType(
                    "timestamp with time zone")
                .IsRequired();


            entity.HasOne(x =>
                    x.BehavioralObservation)
                .WithMany(x =>
                    x.Interventions)
                .HasForeignKey(x =>
                    x.BehavioralObservationId)
                .OnDelete(
                    DeleteBehavior.Cascade);


            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x =>
                    x.CreatedByUserId)
                .OnDelete(
                    DeleteBehavior.Restrict);
        });

        // =========================
        // Resultados de intervención
        // =========================

        modelBuilder.Entity<InterventionResult>(entity =>
        {
            entity.ToTable("resultados_intervencion");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.Id)
                .HasColumnName("id");


            entity.Property(x => x.InterventionId)
                .HasColumnName("intervencion_id")
                .IsRequired();


            entity.Property(x => x.CreatedByUserId)
                .HasColumnName("creado_por_usuario_id")
                .IsRequired();


            entity.Property(x => x.ResultDateTime)
                .HasColumnName("fecha_hora_resultado")
                .HasColumnType(
                    "timestamp without time zone")
                .IsRequired();


            entity.Property(x => x.Outcome)
                .HasColumnName("resultado")
                .HasMaxLength(1500)
                .IsRequired();


            entity.Property(x => x.Effectiveness)
                .HasColumnName("efectividad")
                .HasMaxLength(50)
                .IsRequired();


            entity.Property(x => x.Notes)
                .HasColumnName("notas")
                .HasMaxLength(1500);


            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("fecha_creacion_utc")
                .HasColumnType(
                    "timestamp with time zone")
                .IsRequired();


            entity.HasOne(x =>
                    x.Intervention)
                .WithMany(x =>
                    x.Results)
                .HasForeignKey(x =>
                    x.InterventionId)
                .OnDelete(
                    DeleteBehavior.Cascade);


            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x =>
                    x.CreatedByUserId)
                .OnDelete(
                    DeleteBehavior.Restrict);
        });
    }
    
}