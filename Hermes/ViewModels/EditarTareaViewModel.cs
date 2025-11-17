using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class EditarTareaViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly UsuarioService _usuarioService;
        private Tarea _tarea = new();
        private Tarea _tareaOriginal = new();
        private string _mensajeError = string.Empty;
        private ObservableCollection<Usuario> _usuariosReceptores = new();
        private Usuario? _usuarioReceptorSeleccionado;
        private string _estadoSeleccionado = string.Empty;
        private string _prioridadSeleccionada = string.Empty;
        private string _nombreUsuarioEmisor = string.Empty;

        // Propiedades para selección de hora
        private int _horaInicio;
        private int _minutoInicio;
        private int? _horaLimite;
        private int? _minutoLimite;

        public Tarea Tarea
        {
            get => _tarea;
            set => SetProperty(ref _tarea, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public ObservableCollection<Usuario> UsuariosReceptores
        {
            get => _usuariosReceptores;
            set => SetProperty(ref _usuariosReceptores, value);
        }

        public Usuario? UsuarioReceptorSeleccionado
        {
            get => _usuarioReceptorSeleccionado;
            set => SetProperty(ref _usuarioReceptorSeleccionado, value);
        }

        public string EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set => SetProperty(ref _estadoSeleccionado, value);
        }

        public string PrioridadSeleccionada
        {
            get => _prioridadSeleccionada;
            set => SetProperty(ref _prioridadSeleccionada, value);
        }

        public string NombreUsuarioEmisor
        {
            get => _nombreUsuarioEmisor;
            set => SetProperty(ref _nombreUsuarioEmisor, value);
        }

        public int HoraInicio
        {
            get => _horaInicio;
            set => SetProperty(ref _horaInicio, value);
        }

        public int MinutoInicio
        {
            get => _minutoInicio;
            set => SetProperty(ref _minutoInicio, value);
        }

        public int? HoraLimite
        {
            get => _horaLimite;
            set => SetProperty(ref _horaLimite, value);
        }

        public int? MinutoLimite
        {
            get => _minutoLimite;
            set => SetProperty(ref _minutoLimite, value);
        }

        // Listas para ComboBoxes
        public List<string> Estados { get; } = new()
        {
            "Pendiente",
            "Completado",
            "Vencido",
            "Observado"
        };

        public List<string> Prioridades { get; } = new()
        {
            "Baja",
            "Media",
            "Alta",
            "Urgente"
        };

        public List<int> Horas { get; } = Enumerable.Range(0, 24).ToList();
        public List<int> Minutos { get; } = Enumerable.Range(0, 60).ToList();

        public ICommand ActualizarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarTareaViewModel(Tarea tareaAEditar)
        {
            _tareaService = new TareaService();
            _usuarioService = new UsuarioService();

            // Guardar referencia al objeto original
            _tareaOriginal = tareaAEditar;

            // Crear copia para editar
            Tarea = new Tarea
            {
                IdTarea = tareaAEditar.IdTarea,
                UsuarioEmisorId = tareaAEditar.UsuarioEmisorId,
                UsuarioReceptorId = tareaAEditar.UsuarioReceptorId,
                TituloTarea = tareaAEditar.TituloTarea,
                DescripcionTarea = tareaAEditar.DescripcionTarea,
                EstadoTarea = tareaAEditar.EstadoTarea,
                PrioridadTarea = tareaAEditar.PrioridadTarea,
                FechaInicioTarea = tareaAEditar.FechaInicioTarea,
                FechaLimiteTarea = tareaAEditar.FechaLimiteTarea,
                FechaCompletadaTarea = tareaAEditar.FechaCompletadaTarea
            };

            EstadoSeleccionado = tareaAEditar.EstadoTarea;
            PrioridadSeleccionada = tareaAEditar.PrioridadTarea;

            // Extraer hora y minutos de las fechas existentes
            HoraInicio = tareaAEditar.FechaInicioTarea.Hour;
            MinutoInicio = tareaAEditar.FechaInicioTarea.Minute;

            if (tareaAEditar.FechaLimiteTarea.HasValue)
            {
                HoraLimite = tareaAEditar.FechaLimiteTarea.Value.Hour;
                MinutoLimite = tareaAEditar.FechaLimiteTarea.Value.Minute;
            }

            ActualizarCommand = new RelayCommand(async _ => await ActualizarAsync());
            CancelarCommand = new RelayCommand(_ => Cancelar());

            // Cargar usuarios receptores (excluyendo al emisor original)
            Task.Run(async () => await CargarUsuariosReceptoresAsync());
        }

        private async Task CargarUsuariosReceptoresAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Obtener nombre del emisor original
                var usuarioEmisor = usuarios.FirstOrDefault(u => u.IdUsuario == Tarea.UsuarioEmisorId);
                if (usuarioEmisor?.Empleado != null)
                {
                    NombreUsuarioEmisor = $"{usuarioEmisor.Empleado.NombresEmpleado} {usuarioEmisor.Empleado.ApellidosEmpleado}";
                }

                // Cargar receptores (excluyendo al emisor)
                // VALIDACIÓN CRÍTICA: Solo usuarios Y empleados activos
                UsuariosReceptores.Clear();
                foreach (var usuario in usuarios.Where(u =>
                    u.EsActivoUsuario &&
                    u.Empleado != null &&
                    u.Empleado.EsActivoEmpleado &&
                    u.IdUsuario != Tarea.UsuarioEmisorId))
                {
                    UsuariosReceptores.Add(usuario);
                }

                // Seleccionar receptor actual
                UsuarioReceptorSeleccionado = UsuariosReceptores.FirstOrDefault(u => u.IdUsuario == Tarea.UsuarioReceptorId);
            });
        }

        private async Task ActualizarAsync()
        {
            MensajeError = string.Empty;

            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null)
            {
                MensajeError = "No hay un usuario logueado";
                return;
            }

            if (!usuarioActual.EsActivoUsuario)
            {
                MensajeError = "Su usuario está inactivo. No puede realizar esta acción.";
                return;
            }

            if (usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MensajeError = "Su empleado está inactivo. No puede realizar esta acción.";
                return;
            }

            // Validaciones
            if (UsuarioReceptorSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un usuario receptor";
                return;
            }

            // VALIDACIÓN CRÍTICA: Verificar que el receptor esté activo
            if (!UsuarioReceptorSeleccionado.EsActivoUsuario)
            {
                MensajeError = "El usuario receptor seleccionado está inactivo. Seleccione otro receptor.";
                return;
            }

            if (UsuarioReceptorSeleccionado.Empleado == null || !UsuarioReceptorSeleccionado.Empleado.EsActivoEmpleado)
            {
                MensajeError = "El empleado receptor seleccionado está inactivo. Seleccione otro receptor.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Tarea.TituloTarea))
            {
                MensajeError = "El titulo de la tarea es obligatorio";
                return;
            }

            if (Tarea.FechaLimiteTarea.HasValue && Tarea.FechaLimiteTarea < Tarea.FechaInicioTarea)
            {
                MensajeError = "La fecha limite no puede ser anterior a la fecha de inicio";
                return;
            }

            // Asignar receptor y estados (el emisor no cambia)
            Tarea.UsuarioReceptorId = UsuarioReceptorSeleccionado.IdUsuario;
            Tarea.EstadoTarea = EstadoSeleccionado;
            Tarea.PrioridadTarea = PrioridadSeleccionada;

            // Combinar fecha y hora para FechaInicioTarea
            if (Tarea.FechaInicioTarea != default)
            {
                Tarea.FechaInicioTarea = new DateTime(
                    Tarea.FechaInicioTarea.Year,
                    Tarea.FechaInicioTarea.Month,
                    Tarea.FechaInicioTarea.Day,
                    HoraInicio,
                    MinutoInicio,
                    0);
            }

            // Combinar fecha y hora para FechaLimiteTarea (si existe)
            if (Tarea.FechaLimiteTarea.HasValue && HoraLimite.HasValue && MinutoLimite.HasValue)
            {
                Tarea.FechaLimiteTarea = new DateTime(
                    Tarea.FechaLimiteTarea.Value.Year,
                    Tarea.FechaLimiteTarea.Value.Month,
                    Tarea.FechaLimiteTarea.Value.Day,
                    HoraLimite.Value,
                    MinutoLimite.Value,
                    0);
            }

            // Actualizar en BD
            var resultado = await _tareaService.ActualizarAsync(Tarea);

            if (resultado)
            {
                // Actualizar el objeto original en memoria (el emisor no cambia)
                _tareaOriginal.UsuarioReceptorId = Tarea.UsuarioReceptorId;
                _tareaOriginal.TituloTarea = Tarea.TituloTarea;
                _tareaOriginal.DescripcionTarea = Tarea.DescripcionTarea;
                _tareaOriginal.EstadoTarea = Tarea.EstadoTarea;
                _tareaOriginal.PrioridadTarea = Tarea.PrioridadTarea;
                _tareaOriginal.FechaInicioTarea = Tarea.FechaInicioTarea;
                _tareaOriginal.FechaLimiteTarea = Tarea.FechaLimiteTarea;
                _tareaOriginal.FechaCompletadaTarea = Tarea.FechaCompletadaTarea;

                MessageBox.Show("Tarea actualizada exitosamente",
                              "Exito",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                var ventana = Application.Current.Windows.OfType<Views.EditarTareaWindow>().FirstOrDefault();
                if (ventana != null)
                {
                    ventana.DialogResult = true;
                    ventana.Close();
                }
            }
            else
            {
                MensajeError = "Error al actualizar la tarea";
            }
        }

        private void Cancelar()
        {
            var ventana = Application.Current.Windows.OfType<Views.EditarTareaWindow>().FirstOrDefault();
            if (ventana != null)
            {
                ventana.DialogResult = false;
                ventana.Close();
            }
        }
    }
}
