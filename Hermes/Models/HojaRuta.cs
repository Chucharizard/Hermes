namespace Hermes.Models
{
    public class HojaRuta
    {
        public Guid IdHojaRuta { get; set; }
        public Guid TipoTramiteId { get; set; }
        public Guid UsuarioId { get; set; }
        public string TituloHojaRuta { get; set; } = string.Empty;
        public string EstadoHojaRuta { get; set; } = string.Empty;
        public DateTime FechaInicioHojaRuta { get; set; }
        public DateTime? FechaFinHojaRuta { get; set; }

        // Navegación (sin inversas para evitar relaciones ambiguas)
        public virtual TipoTramite? TipoTramite { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
