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
        private bool _puedeEnviarTareas = false;

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

        public bool PuedeEnviarTareas
        {
            get => _puedeEnviarTareas;
            set => SetProperty(ref _puedeEnviarTareas, value);
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

            // Verificar si el usuario puede enviar tareas
            PuedeEnviarTareas = VerificarPermisoEnvioTareas(usuarioActual);

            if (!PuedeEnviarTareas)
            {
                MensajeError = "Su rol no tiene permisos para enviar tareas. Solo puede recibir tareas.";
            }

            Tarea = new Tarea
            {
                IdTarea = Guid.NewGuid(),
                FechaInicioTarea = DateTime.Now,
                EstadoTarea = "Pendiente",
                PrioridadTarea = "Media"
            };

            GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => PuedeEnviarTareas);
            CancelarCommand = new RelayCommand(_ => Cancelar());

            // Cargar usuarios receptores (filtrados por rol)
            Task.Run(async () => await CargarUsuariosReceptoresAsync());
        }

        private bool VerificarPermisoEnvioTareas(Usuario? usuario)
        {
            if (usuario?.Rol == null)
                return false;

            var rolNombre = usuario.Rol.NombreRol;

            // Solo Broker, Secretaria y Abogada pueden enviar tareas
            return rolNombre.Equals("Broker", StringComparison.OrdinalIgnoreCase) ||
                   rolNombre.Equals("Secretaria", StringComparison.OrdinalIgnoreCase) ||
                   rolNombre.Equals("Abogada", StringComparison.OrdinalIgnoreCase);
        }

        private async Task CargarUsuariosReceptoresAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            var usuarioActual = App.UsuarioActual;

            if (usuarioActual?.Rol == null)
                return;

            var rolActual = usuarioActual.Rol.NombreRol;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UsuariosReceptores.Clear();

                // Filtrar usuarios basándose en el rol del usuario actual
                foreach (var usuario in usuarios.Where(u => u.EsActivoUsuario && u.IdUsuario != usuarioActual.IdUsuario))
                {
                    if (rolActual.Equals("Broker", StringComparison.OrdinalIgnoreCase) ||
                        rolActual.Equals("Secretaria", StringComparison.OrdinalIgnoreCase))
                    {
                        // Broker y Secretaria pueden enviar tareas a todos los roles
                        UsuariosReceptores.Add(usuario);
                    }
                    else if (rolActual.Equals("Abogada", StringComparison.OrdinalIgnoreCase))
                    {
                        // Abogada solo puede enviar tareas a Secretaria y otros roles (no a Broker)
                        if (!usuario.Rol?.NombreRol.Equals("Broker", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            UsuariosReceptores.Add(usuario);
                        }
                    }
                    // Asesores y Administrativos no pueden enviar tareas (no se agregan receptores)
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

            // Verificar permiso de envío
            if (!PuedeEnviarTareas)
            {
                MensajeError = "Su rol no tiene permisos para enviar tareas";
                return;
            }

            // Validaciones
            if (UsuarioReceptorSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un usuario receptor";
                return;
            }

            // Validación adicional: verificar que el receptor sea válido según el rol del emisor
            if (usuarioActual.Rol != null && usuarioActual.Rol.NombreRol.Equals("Abogada", StringComparison.OrdinalIgnoreCase))
            {
                if (UsuarioReceptorSeleccionado.Rol?.NombreRol.Equals("Broker", StringComparison.OrdinalIgnoreCase) == true)
                {
                    MensajeError = "Como Abogada no puede enviar tareas al Broker";
                    return;
                }
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
