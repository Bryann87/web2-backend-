using Microsoft.EntityFrameworkCore;
using academia.Models;

namespace academia.Data
{
    public class AcademiaContext : DbContext
    {
        public AcademiaContext(DbContextOptions<AcademiaContext> options)
            : base(options) { }

        public DbSet<Estado> Estados { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<EstiloDanza> EstilosDanza { get; set; }
        public DbSet<Clase> Clases { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para PostgreSQL - manejar fechas como UTC
            modelBuilder.Entity<Asistencia>()
                .Property(a => a.FechaAsis)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Clase>()
                .Property(c => c.FechaInicio)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Clase>()
                .Property(c => c.FechaFin)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Cobro>()
                .Property(c => c.FechaPago)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Cobro>()
                .Property(c => c.FechaVencimiento)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Cobro>()
                .Property(c => c.FechaCreacion)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Inscripcion>()
                .Property(i => i.FechaInsc)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Inscripcion>()
                .Property(i => i.FechaBaja)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Persona>()
                .Property(p => p.FechaNacimiento)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Persona>()
                .Property(p => p.FechaContrato)
                .HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            modelBuilder.Entity<Persona>()
                .Property(p => p.FechaCreacion)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Persona>()
                .Property(p => p.FechaActualizacion)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Estado>()
                .Property(e => e.FechaCreacion)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Estado>()
                .Property(e => e.FechaActualizacion)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // Relaciones de Persona
            // Representante -> Estudiante representado
            modelBuilder.Entity<Persona>()
                .HasOne(p => p.EstudianteRepresentado)
                .WithMany(p => p.Representantes)
                .HasForeignKey(p => p.IdEstudianteRepresentado)
                .OnDelete(DeleteBehavior.SetNull);

            // Profesor -> Clases que imparte
            modelBuilder.Entity<Clase>()
                .HasOne(c => c.Profesor)
                .WithMany(p => p.ClasesImpartidas)
                .HasForeignKey(c => c.IdProfesor)
                .OnDelete(DeleteBehavior.SetNull);

            // Estudiante -> Inscripciones
            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Estudiante)
                .WithMany(p => p.Inscripciones)
                .HasForeignKey(i => i.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            // Estudiante -> Cobros
            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Estudiante)
                .WithMany(p => p.Cobros)
                .HasForeignKey(c => c.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            // Estudiante -> Asistencias
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Estudiante)
                .WithMany(p => p.Asistencias)
                .HasForeignKey(a => a.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            // Clase -> Inscripciones
            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Clase)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.IdClase)
                .OnDelete(DeleteBehavior.Cascade);

            // Clase -> Asistencias
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Clase)
                .WithMany(c => c.Asistencias)
                .HasForeignKey(a => a.IdClase)
                .OnDelete(DeleteBehavior.Cascade);

            // EstiloDanza -> Clases
            modelBuilder.Entity<Clase>()
                .HasOne(c => c.EstiloDanza)
                .WithMany(ed => ed.Clases)
                .HasForeignKey(c => c.IdEstilo)
                .OnDelete(DeleteBehavior.SetNull);

            // Estado -> Persona
            modelBuilder.Entity<Estado>()
                .HasOne(e => e.Persona)
                .WithMany(p => p.Estados)
                .HasForeignKey(e => e.IdPersona)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(a => a.FechaOperacion)
                    .HasConversion(
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                // Índices para búsquedas eficientes en auditoría
                entity.HasIndex(a => a.TablaAfectada);
                entity.HasIndex(a => a.FechaOperacion);
                entity.HasIndex(a => a.IdUsuario);
                entity.HasIndex(a => new { a.TablaAfectada, a.IdRegistro });
            });

            // Índice único para correo de Persona
            modelBuilder.Entity<Persona>()
                .HasIndex(p => p.Correo)
                .IsUnique();
        }
    }
}
