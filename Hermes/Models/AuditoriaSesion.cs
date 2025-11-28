using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("AUDITORIA_SESION")]
    public class AuditoriaSesion
    {
        [Key]
        [Column("id_auditoria_sesion")]
        public Guid IdAuditoriaSesion { get; set; }

        [Required]
        [Column("usuario_id")]
        public Guid UsuarioId { get; set; }

        [Required]
        [Column("ci_empleado")]
        public int CiEmpleado { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [Column("nombre_maquina")]
        [MaxLength(100)]
        public string NombreMaquina { get; set; } = string.Empty;

        [Required]
        [Column("tipo_evento")]
        [MaxLength(20)]
        public string TipoEvento { get; set; } = string.Empty; // "LOGIN" o "LOGOUT"

        // Navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("CiEmpleado")]
        public virtual Empleado? Empleado { get; set; }
    }
}
