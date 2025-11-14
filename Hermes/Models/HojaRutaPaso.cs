namespace Hermes.Models
{
    public class HojaRutaPaso
    {
        public Guid IdHojaRutaPaso { get; set; }
        public Guid HojaRutaId { get; set; }
        public Guid UsuarioEmisorId { get; set; }
        public Guid UsuarioReceptorId { get; set; }
        public int NumeroPasoHojaRutaPaso { get; set; }
        public string EstadoHojaRutaPaso { get; set; } = string.Empty;
        public DateTime FechaEnvioHojaRutaPaso { get; set; }
        public DateTime? FechaCompletadoHojaRutaPaso { get; set; }

        // Navegación
        public virtual HojaRuta? HojaRuta { get; set; }
        public virtual Usuario? UsuarioEmisor { get; set; }
        public virtual Usuario? UsuarioReceptor { get; set; }
    }
}
