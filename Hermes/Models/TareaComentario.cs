using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("TAREA_COMENTARIO")]
    public class TareaComentario
    {
        [Key]
        [Column("id_comentario")]
        public Guid IdComentario { get; set; }

        [Required]
        [Column("id_tarea")]
        public Guid IdTarea { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("comentario")]
        public string Comentario { get; set; } = string.Empty;

        [Required]
        [Column("fecha_comentario")]
        public DateTime FechaComentario { get; set; }

        // Navigation properties
        [ForeignKey("IdTarea")]
        public virtual Tarea? Tarea { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
