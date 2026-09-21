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

    public DbSet<Institution> Institutions => Set<Institution>();

    public DbSet<Site> Sites => Set<Site>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    // Relación entre usuarios y sedes autorizadas.
    public DbSet<UsuarioSede> UsuariosSedes => Set<UsuarioSede>();


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
    }
}