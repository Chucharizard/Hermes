using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hermes.Models
{
    public class Tarea : INotifyPropertyChanged
    {
        private Guid _idTarea;
        private Guid _usuarioEmisorId;
        private Guid _usuarioReceptorId;
        private string _tituloTarea = string.Empty;
        private string? _descripcionTarea;
        private string _estadoTarea = string.Empty;
        private string _prioridadTarea = string.Empty;
        private DateTime _fechaInicioTarea;
        private DateTime? _fechaLimiteTarea;
        private DateTime? _fechaCompletadaTarea;

        public Guid IdTarea
        {
            get => _idTarea;
            set
            {
                if (_idTarea != value)
                {
                    _idTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid UsuarioEmisorId
        {
            get => _usuarioEmisorId;
            set
            {
                if (_usuarioEmisorId != value)
                {
                    _usuarioEmisorId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid UsuarioReceptorId
        {
            get => _usuarioReceptorId;
            set
            {
                if (_usuarioReceptorId != value)
                {
                    _usuarioReceptorId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TituloTarea
        {
            get => _tituloTarea;
            set
            {
                if (_tituloTarea != value)
                {
                    _tituloTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? DescripcionTarea
        {
            get => _descripcionTarea;
            set
            {
                if (_descripcionTarea != value)
                {
                    _descripcionTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EstadoTarea
        {
            get => _estadoTarea;
            set
            {
                if (_estadoTarea != value)
                {
                    _estadoTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PrioridadTarea
        {
            get => _prioridadTarea;
            set
            {
                if (_prioridadTarea != value)
                {
                    _prioridadTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime FechaInicioTarea
        {
            get => _fechaInicioTarea;
            set
            {
                if (_fechaInicioTarea != value)
                {
                    _fechaInicioTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? FechaLimiteTarea
        {
            get => _fechaLimiteTarea;
            set
            {
                if (_fechaLimiteTarea != value)
                {
                    _fechaLimiteTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? FechaCompletadaTarea
        {
            get => _fechaCompletadaTarea;
            set
            {
                if (_fechaCompletadaTarea != value)
                {
                    _fechaCompletadaTarea = value;
                    OnPropertyChanged();
                }
            }
        }

        // Navegación
        public virtual Usuario? UsuarioEmisor { get; set; }
        public virtual Usuario? UsuarioReceptor { get; set; }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
