using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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

        // Propiedades calculadas (no se guardan en BD)

        /// <summary>
        /// Indica si la tarea está vencida (pasó su fecha límite y no está completada/archivada)
        /// </summary>
        [NotMapped]
        public bool EstaVencida
        {
            get
            {
                if (!FechaLimiteTarea.HasValue)
                    return false;

                if (EstadoTarea == "Completado" || EstadoTarea == "Archivado")
                    return false;

                return DateTime.Now > FechaLimiteTarea.Value;
            }
        }

        /// <summary>
        /// Indica si la tarea está próxima a vencer (menos de 24 horas)
        /// </summary>
        [NotMapped]
        public bool ProximaAVencer
        {
            get
            {
                if (!FechaLimiteTarea.HasValue || EstaVencida)
                    return false;

                if (EstadoTarea == "Completado" || EstadoTarea == "Archivado")
                    return false;

                var horasRestantes = (FechaLimiteTarea.Value - DateTime.Now).TotalHours;
                return horasRestantes > 0 && horasRestantes <= 24;
            }
        }

        /// <summary>
        /// Tiempo restante o tiempo de retraso en formato legible
        /// </summary>
        [NotMapped]
        public string TiempoRestante
        {
            get
            {
                if (!FechaLimiteTarea.HasValue)
                    return "Sin fecha límite";

                if (EstadoTarea == "Completado")
                    return "Completada";

                if (EstadoTarea == "Archivado")
                    return "Archivada";

                var diferencia = FechaLimiteTarea.Value - DateTime.Now;

                if (diferencia.TotalMinutes < 0)
                {
                    // Vencida - mostrar tiempo de retraso
                    var retraso = DateTime.Now - FechaLimiteTarea.Value;

                    if (retraso.TotalDays >= 1)
                        return $"⚠️ Vencida hace {(int)retraso.TotalDays} día(s)";
                    else if (retraso.TotalHours >= 1)
                        return $"⚠️ Vencida hace {(int)retraso.TotalHours} hora(s)";
                    else
                        return $"⚠️ Vencida hace {(int)retraso.TotalMinutes} minuto(s)";
                }
                else
                {
                    // No vencida - mostrar tiempo restante
                    if (diferencia.TotalDays >= 1)
                        return $"⏰ {(int)diferencia.TotalDays} día(s) restantes";
                    else if (diferencia.TotalHours >= 1)
                        return $"⚠️ {(int)diferencia.TotalHours} hora(s) restantes";
                    else
                        return $"🔥 {(int)diferencia.TotalMinutes} minuto(s) restantes";
                }
            }
        }

        /// <summary>
        /// Color sugerido para la UI basado en el estado de vencimiento
        /// </summary>
        [NotMapped]
        public string ColorVencimiento
        {
            get
            {
                if (EstadoTarea == "Completado")
                    return "#27AE60"; // Verde

                if (EstadoTarea == "Archivado")
                    return "#95A5A6"; // Gris

                if (EstaVencida)
                    return "#E74C3C"; // Rojo

                if (ProximaAVencer)
                    return "#F39C12"; // Naranja

                return "#3498DB"; // Azul (normal)
            }
        }

        /// <summary>
        /// Indica si fue completada con retraso
        /// </summary>
        [NotMapped]
        public bool CompletadaConRetraso
        {
            get
            {
                if (EstadoTarea != "Completado" || !FechaCompletadaTarea.HasValue || !FechaLimiteTarea.HasValue)
                    return false;

                return FechaCompletadaTarea.Value > FechaLimiteTarea.Value;
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
