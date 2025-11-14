namespace Hermes.Models
{
    public class Tarea
    {
        public Guid IdTarea { get; set; }
        public Guid UsuarioEmisorId { get; set; }
        public Guid UsuarioReceptorId { get; set; }
        public string TituloTarea { get; set; } = string.Empty;
        public string? DescripcionTarea { get; set; }
        public string EstadoTarea { get; set; } = string.Empty;
        public string PrioridadTarea { get; set; } = string.Empty;
        public DateTime FechaInicioTarea { get; set; }
        public DateTime? FechaLimiteTarea { get; set; }
        public DateTime? FechaCompletadaTarea { get; set; }

        // Navegación
        public virtual Usuario? UsuarioEmisor { get; set; }
        public virtual Usuario? UsuarioReceptor { get; set; }
    }
}
