using Microsoft.EntityFrameworkCore;
using Hermes.Models;

namespace Hermes.Data
{
    public class HermesDbContext : DbContext
    {
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=PANADERO\PANCITO;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración Empleado
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.ToTable("EMPLEADO");
                entity.HasKey(e => e.CiEmpleado);
                entity.Property(e => e.CiEmpleado).HasColumnName("ci_empleado");
                entity.Property(e => e.NombresEmpleado).HasColumnName("nombres_empleado").HasMaxLength(100);
                entity.Property(e => e.ApellidosEmpleado).HasColumnName("apellidos_empleado").HasMaxLength(100);
                entity.Property(e => e.TelefonoEmpleado).HasColumnName("telefono_empleado").HasMaxLength(20);
                entity.Property(e => e.CorreoEmpleado).HasColumnName("correo_empleado").HasMaxLength(320);
                entity.Property(e => e.EsActivoEmpleado).HasColumnName("es_activo_empleado");
            });

            // Configuración Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("ROL");
                entity.HasKey(r => r.IdRol);
                entity.Property(r => r.IdRol).HasColumnName("id_rol").HasDefaultValueSql("NEWID()");
                entity.Property(r => r.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50);
                entity.Property(r => r.DescripcionRol).HasColumnName("descripcion_rol").HasMaxLength(300);
                entity.Property(r => r.EsActivoRol).HasColumnName("es_activo_rol");
            });

            // Configuración Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("USUARIO");
                entity.HasKey(u => u.IdUsuario);
                entity.Property(u => u.IdUsuario).HasColumnName("id_usuario").HasDefaultValueSql("NEWID()");
                entity.Property(u => u.EmpleadoCi).HasColumnName("empleado_ci");
                entity.Property(u => u.RolId).HasColumnName("rol_id");
                entity.Property(u => u.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(50);
                entity.Property(u => u.PasswordUsuario).HasColumnName("password_usuario").HasMaxLength(255);
                entity.Property(u => u.EsActivoUsuario).HasColumnName("es_activo_usuario");

                // Relaciones
                entity.HasOne(u => u.Empleado)
                    .WithMany(e => e.Usuarios)
                    .HasForeignKey(u => u.EmpleadoCi)
                    .HasConstraintName("FK_USUARIO_EMPLEADO");

                entity.HasOne(u => u.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(u => u.RolId)
                    .HasConstraintName("FK_USUARIO_ROL");
            });
        }
    }
}
