namespace Hermes.Models
{
    public class Usuario
    {
        public Guid IdUsuario { get; set; }
        public int EmpleadoCi { get; set; }
        public Guid RolId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string PasswordUsuario { get; set; } = string.Empty;
        public bool EsActivoUsuario { get; set; }
        public string TemaPreferido { get; set; } = "Emerald"; // Tema por defecto: Verde Esmeralda

        // Navegaci�n
        public virtual Empleado? Empleado { get; set; }
        public virtual Rol? Rol { get; set; }
    }
}
