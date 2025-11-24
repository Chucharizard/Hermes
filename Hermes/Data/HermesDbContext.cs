using Microsoft.EntityFrameworkCore;
using Hermes.Models;

namespace Hermes.Data
{
    public class HermesDbContext : DbContext
    {
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoTramite> TiposTramite { get; set; }
        public DbSet<HojaRuta> HojasRuta { get; set; }
        public DbSet<HojaRutaPaso> HojasRutaPaso { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<TareaComentario> TareasComentarios { get; set; }
        public DbSet<TareaAdjunto> TareasAdjuntos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=pan\SQLEXPRESS;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;",
                options => options.EnableRetryOnFailure()
            );

            // Configurar para que solo mapee las entidades definidas explícitamente
            optionsBuilder.EnableSensitiveDataLogging(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraci�n Empleado
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

            // Configuraci�n Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("ROL");
                entity.HasKey(r => r.IdRol);
                entity.Property(r => r.IdRol).HasColumnName("id_rol").HasDefaultValueSql("NEWID()");
                entity.Property(r => r.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50);
                entity.Property(r => r.DescripcionRol).HasColumnName("descripcion_rol").HasMaxLength(300);
                entity.Property(r => r.EsActivoRol).HasColumnName("es_activo_rol");
            });

            // Configuraci�n Usuario
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
                entity.Property(u => u.TemaPreferido).HasColumnName("tema_preferido").HasMaxLength(50).HasDefaultValue("Emerald");

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

            // Configuraci�n TipoTramite
            modelBuilder.Entity<TipoTramite>(entity =>
            {
                entity.ToTable("TIPO_TRAMITE");
                entity.HasKey(t => t.IdTipoTramite);
                entity.Property(t => t.IdTipoTramite).HasColumnName("id_tipo_tramite").HasDefaultValueSql("NEWID()");
                entity.Property(t => t.NombreTipoTramite).HasColumnName("nombre_tipo_tramite").HasMaxLength(100);
                entity.Property(t => t.DescripcionTipoTramite).HasColumnName("descripcion_tipo_tramite").HasMaxLength(300);
                entity.Property(t => t.FechaCreacionTipoTramite).HasColumnName("fecha_creacion_tipo_tramite");
            });

            // Configuraci�n HojaRuta
            modelBuilder.Entity<HojaRuta>(entity =>
            {
                entity.ToTable("HOJA_RUTA");
                entity.HasKey(h => h.IdHojaRuta);
                entity.Property(h => h.IdHojaRuta).HasColumnName("id_hoja_ruta").HasDefaultValueSql("NEWID()");
                entity.Property(h => h.TipoTramiteId).HasColumnName("tipo_tramite_id");
                entity.Property(h => h.UsuarioId).HasColumnName("usuario_id");
                entity.Property(h => h.TituloHojaRuta).HasColumnName("titulo_hoja_ruta").HasMaxLength(150);
                entity.Property(h => h.EstadoHojaRuta).HasColumnName("estado_hoja_ruta").HasMaxLength(50);
                entity.Property(h => h.FechaInicioHojaRuta).HasColumnName("fecha_inicio_hoja_ruta");
                entity.Property(h => h.FechaFinHojaRuta).HasColumnName("fecha_fin_hoja_ruta");

                entity.HasOne(h => h.TipoTramite)
                    .WithMany()
                    .HasForeignKey(h => h.TipoTramiteId)
                    .HasConstraintName("FK_HOJA_RUTA_TIPO")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(h => h.Usuario)
                    .WithMany()
                    .HasForeignKey(h => h.UsuarioId)
                    .HasConstraintName("FK_HOJA_RUTA_USUARIO")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuraci�n HojaRutaPaso (m�ltiples FK a Usuario)
            modelBuilder.Entity<HojaRutaPaso>(entity =>
            {
                entity.ToTable("HOJA_RUTA_PASO");
                entity.HasKey(p => p.IdHojaRutaPaso);
                entity.Property(p => p.IdHojaRutaPaso).HasColumnName("id_hoja_ruta_paso").HasDefaultValueSql("NEWID()");
                entity.Property(p => p.HojaRutaId).HasColumnName("hoja_ruta_id");
                entity.Property(p => p.UsuarioEmisorId).HasColumnName("usuario_emisor_id");
                entity.Property(p => p.UsuarioReceptorId).HasColumnName("usuario_receptor_id");
                entity.Property(p => p.NumeroPasoHojaRutaPaso).HasColumnName("numero_paso_hoja_ruta_paso");
                entity.Property(p => p.EstadoHojaRutaPaso).HasColumnName("estado_hoja_ruta_paso").HasMaxLength(50);
                entity.Property(p => p.FechaEnvioHojaRutaPaso).HasColumnName("fecha_envio_hoja_ruta_paso");
                entity.Property(p => p.FechaCompletadoHojaRutaPaso).HasColumnName("fecha_completado_hoja_ruta_paso");

                entity.HasOne(p => p.HojaRuta)
                    .WithMany()
                    .HasForeignKey(p => p.HojaRutaId)
                    .HasConstraintName("FK_PASO_HOJA_RUTA")
                    .OnDelete(DeleteBehavior.NoAction);

                // Configurar m�ltiples relaciones con Usuario explícitamente
                entity.HasOne(p => p.UsuarioEmisor)
                    .WithMany()
                    .HasForeignKey(p => p.UsuarioEmisorId)
                    .HasConstraintName("FK_PASO_EMISOR")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.UsuarioReceptor)
                    .WithMany()
                    .HasForeignKey(p => p.UsuarioReceptorId)
                    .HasConstraintName("FK_PASO_RECEPTOR")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuraci�n Tarea (m�ltiples FK a Usuario)
            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.ToTable("TAREA");
                entity.HasKey(t => t.IdTarea);
                entity.Property(t => t.IdTarea).HasColumnName("id_tarea").HasDefaultValueSql("NEWID()");
                entity.Property(t => t.UsuarioEmisorId).HasColumnName("usuario_emisor_id");
                entity.Property(t => t.UsuarioReceptorId).HasColumnName("usuario_receptor_id");
                entity.Property(t => t.TituloTarea).HasColumnName("titulo_tarea").HasMaxLength(150);
                entity.Property(t => t.DescripcionTarea).HasColumnName("descripcion_tarea").HasColumnType("NVARCHAR(MAX)");
                entity.Property(t => t.EstadoTarea).HasColumnName("estado_tarea").HasMaxLength(50);
                entity.Property(t => t.PrioridadTarea).HasColumnName("prioridad_tarea").HasMaxLength(20);
                entity.Property(t => t.FechaInicioTarea).HasColumnName("fecha_inicio_tarea");
                entity.Property(t => t.FechaLimiteTarea).HasColumnName("fecha_limite_tarea");
                entity.Property(t => t.FechaCompletadaTarea).HasColumnName("fecha_completada_tarea");
                entity.Property(t => t.PermiteEntregaConRetraso).HasColumnName("permite_entrega_con_retraso").HasDefaultValue(true);

                // Configurar m�ltiples relaciones con Usuario explícitamente
                entity.HasOne(t => t.UsuarioEmisor)
                    .WithMany()
                    .HasForeignKey(t => t.UsuarioEmisorId)
                    .HasConstraintName("FK_TAREA_EMISOR")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.UsuarioReceptor)
                    .WithMany()
                    .HasForeignKey(t => t.UsuarioReceptorId)
                    .HasConstraintName("FK_TAREA_RECEPTOR")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuración TareaComentario
            modelBuilder.Entity<TareaComentario>(entity =>
            {
                // Solo configurar lo que NO está en data annotations
                entity.Property(tc => tc.IdTareaComentario).HasDefaultValueSql("NEWID()");

                entity.HasOne(tc => tc.Tarea)
                    .WithMany()
                    .HasForeignKey(tc => tc.TareaIdComentario)
                    .HasConstraintName("FK_COMENTARIO_TAREA")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tc => tc.Usuario)
                    .WithMany()
                    .HasForeignKey(tc => tc.UsuarioAutorId)
                    .HasConstraintName("FK_COMENTARIO_USUARIO")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuración TareaAdjunto
            modelBuilder.Entity<TareaAdjunto>(entity =>
            {
                // Solo configurar lo que NO está en data annotations
                entity.Property(ta => ta.IdTareaAdjunto).HasDefaultValueSql("NEWID()");

                entity.HasOne(ta => ta.Tarea)
                    .WithMany()
                    .HasForeignKey(ta => ta.TareaId)
                    .HasConstraintName("FK_TAREA_ADJUNTO")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                // Relación opcional con Usuario (puede ser null)
                entity.HasOne(ta => ta.UsuarioSubioNavigation)
                    .WithMany()
                    .HasForeignKey(ta => ta.IdUsuarioSubioTareaAdjunto)
                    .HasConstraintName("FK_ADJUNTO_USUARIO")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);
            });
        }
    }
}
