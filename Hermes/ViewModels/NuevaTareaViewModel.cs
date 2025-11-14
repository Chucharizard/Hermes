using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Models.Enums;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class NuevaTareaViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly UsuarioService _usuarioService;
        private Tarea _tarea = new();
        private string _mensajeError = string.Empty;
        private ObservableCollection<Usuario> _usuariosReceptores = new();
        private Usuario? _usuarioReceptorSeleccionado;
        private string _estadoSeleccionado = "Pendiente";
        private string _prioridadSeleccionada = "Media";
        private string _nombreUsuarioEmisor = string.Empty;

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

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        public NuevaTareaViewModel()
        {
            _tareaService = new TareaService();
            _usuarioService = new UsuarioService();

            // Obtener el usuario actual (emisor)
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual?.Empleado != null)
            {
                NombreUsuarioEmisor = $"{usuarioActual.Empleado.NombresEmpleado} {usuarioActual.Empleado.ApellidosEmpleado}";
            }

            Tarea = new Tarea
            {
                IdTarea = Guid.NewGuid(),
                FechaInicioTarea = DateTime.Now,
                EstadoTarea = "Pendiente",
                PrioridadTarea = "Media"
            };

            GuardarCommand = new RelayCommand(async _ => await GuardarAsync());
            CancelarCommand = new RelayCommand(_ => Cancelar());

            // Cargar usuarios receptores (excluyendo al usuario actual)
            Task.Run(async () => await CargarUsuariosReceptoresAsync());
        }

        private async Task CargarUsuariosReceptoresAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            var usuarioActual = App.UsuarioActual;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UsuariosReceptores.Clear();
                foreach (var usuario in usuarios.Where(u => u.EsActivoUsuario && u.IdUsuario != usuarioActual?.IdUsuario))
                {
                    UsuariosReceptores.Add(usuario);
                }
            });
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;

            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null)
            {
                MensajeError = "No hay un usuario logueado";
                return;
            }

            // Validaciones
            if (UsuarioReceptorSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un usuario receptor";
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
            Tarea.UsuarioEmisorId = usuarioActual.IdUsuario;  // Usuario logueado
            Tarea.UsuarioReceptorId = UsuarioReceptorSeleccionado.IdUsuario;
            Tarea.EstadoTarea = EstadoSeleccionado;
            Tarea.PrioridadTarea = PrioridadSeleccionada;

            // Guardar
            var resultado = await _tareaService.CrearAsync(Tarea);

            if (resultado)
            {
                MessageBox.Show("Tarea creada exitosamente",
                              "Exito",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                Application.Current.Windows.OfType<Views.NuevaTareaWindow>().FirstOrDefault()?.Close();
            }
            else
            {
                MensajeError = "Error al crear la tarea";
            }
        }

        private void Cancelar()
        {
            Application.Current.Windows.OfType<Views.NuevaTareaWindow>().FirstOrDefault()?.Close();
        }
    }
}
