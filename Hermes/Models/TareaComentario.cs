using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("TareaComentario")]
    public class TareaComentario
    {
        [Key]
        public Guid IdComentario { get; set; }

        [Required]
        public Guid IdTarea { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comentario { get; set; } = string.Empty;

        [Required]
        public DateTime FechaComentario { get; set; }

        // Navigation properties
        [ForeignKey("IdTarea")]
        public virtual Tarea? Tarea { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
