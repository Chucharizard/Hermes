using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models
{
    [Table("AUDITORIA_EMPLEADO_USUARIO")]
    public class AuditoriaEmpleadoUsuario
    {
        [Key]
        [Column("id_auditoria")]
        public Guid IdAuditoria { get; set; }

        [Required]
        [Column("tabla_afectada")]
        [MaxLength(50)]
        public string TablaAfectada { get; set; } = string.Empty; // "EMPLEADO" o "USUARIO"

        [Required]
        [Column("accion")]
        [MaxLength(10)]
        public string Accion { get; set; } = string.Empty; // "INSERT", "UPDATE", "DELETE"

        [Column("ci_empleado_afectado")]
        public int? CiEmpleadoAfectado { get; set; }

        [Column("usuario_id_afectado")]
        public Guid? UsuarioIdAfectado { get; set; }

        [Column("usuario_id_modificador")]
        public Guid? UsuarioIdModificador { get; set; }

        [Column("ci_modificador")]
        public int? CiModificador { get; set; }

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
    }
}
