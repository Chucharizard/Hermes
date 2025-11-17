using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hermes.Models
{
    public class Empleado : INotifyPropertyChanged
    {
        private int _ciEmpleado;
        private string _nombresEmpleado = string.Empty;
        private string _apellidosEmpleado = string.Empty;
        private string _telefonoEmpleado = string.Empty;
        private string _correoEmpleado = string.Empty;
        private bool _esActivoEmpleado;

        public int CiEmpleado
        {
            get => _ciEmpleado;
            set
            {
                if (_ciEmpleado != value)
                {
                    _ciEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NombresEmpleado
        {
            get => _nombresEmpleado;
            set
            {
                if (_nombresEmpleado != value)
                {
                    _nombresEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ApellidosEmpleado
        {
            get => _apellidosEmpleado;
            set
            {
                if (_apellidosEmpleado != value)
                {
                    _apellidosEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TelefonoEmpleado
        {
            get => _telefonoEmpleado;
            set
            {
                if (_telefonoEmpleado != value)
                {
                    _telefonoEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CorreoEmpleado
        {
            get => _correoEmpleado;
            set
            {
                if (_correoEmpleado != value)
                {
                    _correoEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EsActivoEmpleado
        {
            get => _esActivoEmpleado;
            set
            {
                if (_esActivoEmpleado != value)
                {
                    _esActivoEmpleado = value;
                    OnPropertyChanged();
                }
            }
        }

        // Navegación
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
