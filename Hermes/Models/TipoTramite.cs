namespace Hermes.Models
{
    public class TipoTramite
    {
        public Guid IdTipoTramite { get; set; }
        public string NombreTipoTramite { get; set; } = string.Empty;
        public string? DescripcionTipoTramite { get; set; }
        public DateTime FechaCreacionTipoTramite { get; set; }
    }
}
