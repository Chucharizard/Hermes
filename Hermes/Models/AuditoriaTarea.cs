using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("AUDITORIA_TAREA")]
    public class AuditoriaTarea
    {
        [Key]
        [Column("id_auditoria")]
        public Guid IdAuditoria { get; set; }

        [Column("tarea_id")]
        public Guid? TareaId { get; set; }

        [Required]
        [Column("accion")]
        [MaxLength(10)]
        public string Accion { get; set; } = string.Empty; // "INSERT", "UPDATE", "DELETE"

        [Column("usuario_id_modificador")]
        public Guid? UsuarioIdModificador { get; set; }

        [Column("ci_modificador")]
        [MaxLength(20)]
        public string? CiModificador { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [Column("nombre_maquina")]
        [MaxLength(100)]
        public string NombreMaquina { get; set; } = string.Empty;

        [Column("detalles")]
        public string? Detalles { get; set; } // JSON con los cambios

        // Navegación
        [ForeignKey("UsuarioIdModificador")]
        public virtual Usuario? UsuarioModificador { get; set; }

        [ForeignKey("TareaId")]
        public virtual Tarea? Tarea { get; set; }
    }
}
