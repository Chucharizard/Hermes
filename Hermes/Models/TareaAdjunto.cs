using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("TAREA_ADJUNTO")]
    public class TareaAdjunto
    {
        [Key]
        [Column("id_tarea_adjunto")]
        public Guid IdTareaAdjunto { get; set; }

        [Required]
        [Column("tarea_id")]
        public Guid TareaId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("nombre_archivo_tarea_adjunto")]
        public string NombreArchivoTareaAdjunto { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("tipo_archivo_tarea_adjunto")]
        public string TipoArchivoTareaAdjunto { get; set; } = string.Empty;

        [Column("archivo_tarea_adjunto")]
        public byte[]? ArchivoTareaAdjunto { get; set; }

        [Required]
        [Column("fecha_subida_tarea_adjunto")]
        public DateTime FechaSubidaTareaAdjunto { get; set; }

        // Campo adicional para rastrear quién subió el archivo (opcional si no existe en BD)
        [Column("id_usuario_subio")]
        public Guid? IdUsuarioSubioTareaAdjunto { get; set; }

        // Navigation properties
        [ForeignKey("TareaId")]
        public virtual Tarea? Tarea { get; set; }

        [ForeignKey("IdUsuarioSubioTareaAdjunto")]
        public virtual Usuario? UsuarioSubioNavigation { get; set; }

        // Propiedades calculadas para compatibilidad con código existente
        [NotMapped]
        public Guid IdAdjunto
        {
            get => IdTareaAdjunto;
            set => IdTareaAdjunto = value;
        }

        [NotMapped]
        public Guid IdTarea
        {
            get => TareaId;
            set => TareaId = value;
        }

        [NotMapped]
        public string NombreArchivo
        {
            get => NombreArchivoTareaAdjunto;
            set => NombreArchivoTareaAdjunto = value;
        }

        [NotMapped]
        public string TipoArchivo
        {
            get => TipoArchivoTareaAdjunto;
            set => TipoArchivoTareaAdjunto = value;
        }

        [NotMapped]
        public DateTime FechaSubida
        {
            get => FechaSubidaTareaAdjunto;
            set => FechaSubidaTareaAdjunto = value;
        }

        [NotMapped]
        public long TamañoArchivo => ArchivoTareaAdjunto?.Length ?? 0;

        // Propiedad para mantener compatibilidad con código que usa UsuarioSubio
        [NotMapped]
        public virtual Usuario? UsuarioSubio
        {
            get => UsuarioSubioNavigation;
            set => UsuarioSubioNavigation = value;
        }

        [NotMapped]
        public Guid IdUsuarioSubio
        {
            get => IdUsuarioSubioTareaAdjunto ?? Guid.Empty;
            set => IdUsuarioSubioTareaAdjunto = value;
        }
    }
}
