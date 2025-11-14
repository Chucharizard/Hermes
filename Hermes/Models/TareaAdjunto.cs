using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("TareaAdjunto")]
    public class TareaAdjunto
    {
        [Key]
        public Guid IdAdjunto { get; set; }

        [Required]
        public Guid IdTarea { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        [StringLength(100)]
        public string TipoArchivo { get; set; } = string.Empty;

        public long TamañoArchivo { get; set; }

        [Required]
        public Guid IdUsuarioSubio { get; set; }

        [Required]
        public DateTime FechaSubida { get; set; }

        // Navigation properties
        [ForeignKey("IdTarea")]
        public virtual Tarea? Tarea { get; set; }

        [ForeignKey("IdUsuarioSubio")]
        public virtual Usuario? UsuarioSubio { get; set; }
    }
}
