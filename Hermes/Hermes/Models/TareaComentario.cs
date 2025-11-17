using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("TAREA_COMENTARIO")]
    public class TareaComentario
    {
        [Key]
        [Column("id_tarea_comentario")]
        public Guid IdTareaComentario { get; set; }

        [Required]
        [Column("tarea_id")]
        public Guid TareaIdComentario { get; set; }

        [Required]
        [Column("usuario_autor_id")]
        public Guid UsuarioAutorId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("comentario_tarea_comentario")]
        public string ComentarioTareaComentario { get; set; } = string.Empty;

        [Required]
        [Column("fecha_tarea_comentario")]
        public DateTime FechaTareaComentario { get; set; }

        // Navigation properties
        [ForeignKey("TareaIdComentario")]
        public virtual Tarea? Tarea { get; set; }

        [ForeignKey("UsuarioAutorId")]
        public virtual Usuario? Usuario { get; set; }

        // Propiedades de compatibilidad (NotMapped)
        [NotMapped]
        public Guid IdComentario
        {
            get => IdTareaComentario;
            set => IdTareaComentario = value;
        }

        [NotMapped]
        public Guid IdTarea
        {
            get => TareaIdComentario;
            set => TareaIdComentario = value;
        }

        [NotMapped]
        public Guid IdUsuario
        {
            get => UsuarioAutorId;
            set => UsuarioAutorId = value;
        }

        [NotMapped]
        public string Comentario
        {
            get => ComentarioTareaComentario;
            set => ComentarioTareaComentario = value;
        }

        [NotMapped]
        public DateTime FechaComentario
        {
            get => FechaTareaComentario;
            set => FechaTareaComentario = value;
        }
    }
}
