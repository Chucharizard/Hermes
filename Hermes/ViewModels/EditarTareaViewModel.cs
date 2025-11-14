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
        private ObservableCollection<Usuario> _usuarios = new();
        private Usuario? _usuarioEmisorSeleccionado;
        private Usuario? _usuarioReceptorSeleccionado;
        private string _estadoSeleccionado = string.Empty;
        private string _prioridadSeleccionada = string.Empty;

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

        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        public Usuario? UsuarioEmisorSeleccionado
        {
            get => _usuarioEmisorSeleccionado;
            set => SetProperty(ref _usuarioEmisorSeleccionado, value);
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

            ActualizarCommand = new RelayCommand(async _ => await ActualizarAsync());
            CancelarCommand = new RelayCommand(_ => Cancelar());

            // Cargar usuarios
            Task.Run(async () => await CargarUsuariosAsync());
        }

        private async Task CargarUsuariosAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Usuarios.Clear();
                foreach (var usuario in usuarios.Where(u => u.EsActivoUsuario))
                {
                    Usuarios.Add(usuario);
                }

                // Seleccionar usuarios actuales
                UsuarioEmisorSeleccionado = Usuarios.FirstOrDefault(u => u.IdUsuario == Tarea.UsuarioEmisorId);
                UsuarioReceptorSeleccionado = Usuarios.FirstOrDefault(u => u.IdUsuario == Tarea.UsuarioReceptorId);
            });
        }

        private async Task ActualizarAsync()
        {
            MensajeError = string.Empty;

            // Validaciones
            if (UsuarioEmisorSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un usuario emisor";
                return;
            }

            if (UsuarioReceptorSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un usuario receptor";
                return;
            }

            if (UsuarioEmisorSeleccionado.IdUsuario == UsuarioReceptorSeleccionado.IdUsuario)
            {
                MensajeError = "El emisor y receptor no pueden ser el mismo usuario";
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

            // Asignar usuarios y estados
            Tarea.UsuarioEmisorId = UsuarioEmisorSeleccionado.IdUsuario;
            Tarea.UsuarioReceptorId = UsuarioReceptorSeleccionado.IdUsuario;
            Tarea.EstadoTarea = EstadoSeleccionado;
            Tarea.PrioridadTarea = PrioridadSeleccionada;

            // Actualizar en BD
            var resultado = await _tareaService.ActualizarAsync(Tarea);

            if (resultado)
            {
                // Actualizar el objeto original en memoria
                _tareaOriginal.UsuarioEmisorId = Tarea.UsuarioEmisorId;
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
