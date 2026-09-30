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
    }
    
}