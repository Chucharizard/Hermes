using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
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
        private readonly TareaAdjuntoService _adjuntoService;
        private readonly Action? _onSaveCompleted;
        private Tarea _tarea = new();
        private string _mensajeError = string.Empty;
        private ObservableCollection<Usuario> _usuariosReceptores = new();
        private Usuario? _usuarioReceptorSeleccionado;
        private string _estadoSeleccionado = "Pendiente";
        private string _prioridadSeleccionada = "Media";
        private string _nombreUsuarioEmisor = string.Empty;
        private bool _puedeEnviarTareas = false;
        private ObservableCollection<ArchivoPendiente> _archivosPendientes = new();

        // Propiedades para selección de hora
        private int _horaInicio = DateTime.Now.Hour;
        private int _minutoInicio = DateTime.Now.Minute;
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

        public bool PuedeEnviarTareas
        {
            get => _puedeEnviarTareas;
            set => SetProperty(ref _puedeEnviarTareas, value);
        }

        public ObservableCollection<ArchivoPendiente> ArchivosPendientes
        {
            get => _archivosPendientes;
            set => SetProperty(ref _archivosPendientes, value);
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
            "Alta"
        };

        public List<int> Horas { get; } = Enumerable.Range(0, 24).ToList();
        public List<int> Minutos { get; } = Enumerable.Range(0, 60).ToList();

        public ICommand GuardarCommand { get; }
        public ICommand SeleccionarArchivoCommand { get; }
        public ICommand EliminarArchivoCommand { get; }

        public NuevaTareaViewModel(Action? onSaveCompleted = null)
        {
            _tareaService = new TareaService();
            _usuarioService = new UsuarioService();
            _adjuntoService = new TareaAdjuntoService();
            _onSaveCompleted = onSaveCompleted;

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

            // Inicializar hora de inicio con hora actual
            var ahora = DateTime.Now;
            HoraInicio = ahora.Hour;
            MinutoInicio = ahora.Minute;

            Tarea = new Tarea
            {
                IdTarea = Guid.NewGuid(),
                FechaInicioTarea = ahora,
                EstadoTarea = "Pendiente",
                PrioridadTarea = "Media"
            };

            GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => PuedeEnviarTareas);
            SeleccionarArchivoCommand = new RelayCommand(_ => SeleccionarArchivo());
            EliminarArchivoCommand = new RelayCommand(param => EliminarArchivo((ArchivoPendiente)param!));

            // Cargar usuarios receptores (filtrados por rol)
            Task.Run(async () => await CargarUsuariosReceptoresAsync());
        }

        private bool VerificarPermisoEnvioTareas(Usuario? usuario)
        {
            if (usuario?.Rol == null)
                return false;

            var rolNombre = usuario.Rol.NombreRol;

            // Solo Broker, Secretaria y Abogada/Abogado pueden enviar tareas
            return rolNombre.Equals("Broker", StringComparison.OrdinalIgnoreCase) ||
                   rolNombre.Equals("Secretaria", StringComparison.OrdinalIgnoreCase) ||
                   rolNombre.Equals("Abogada", StringComparison.OrdinalIgnoreCase) ||
                   rolNombre.Equals("Abogado", StringComparison.OrdinalIgnoreCase);
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
                // VALIDACIÓN CRÍTICA: Solo usuarios Y empleados activos
                foreach (var usuario in usuarios.Where(u =>
                    u.EsActivoUsuario &&
                    u.Empleado != null &&
                    u.Empleado.EsActivoEmpleado &&
                    u.IdUsuario != usuarioActual.IdUsuario))
                {
                    if (rolActual.Equals("Broker", StringComparison.OrdinalIgnoreCase) ||
                        rolActual.Equals("Secretaria", StringComparison.OrdinalIgnoreCase))
                    {
                        // Broker y Secretaria pueden enviar tareas a todos los roles
                        UsuariosReceptores.Add(usuario);
                    }
                    else if (rolActual.Equals("Abogada", StringComparison.OrdinalIgnoreCase) ||
                             rolActual.Equals("Abogado", StringComparison.OrdinalIgnoreCase))
                    {
                        // Abogada/Abogado SOLO puede enviar tareas a Secretaria
                        if (usuario.Rol?.NombreRol.Equals("Secretaria", StringComparison.OrdinalIgnoreCase) == true)
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

            // VALIDACIÓN CRÍTICA: Verificar que el usuario y empleado emisor estén activos
            if (!usuarioActual.EsActivoUsuario)
            {
                MensajeError = "Su usuario está inactivo. Contacte al administrador.";
                return;
            }

            if (usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MensajeError = "Su empleado está inactivo. Contacte al administrador.";
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

            // VALIDACIÓN CRÍTICA: Verificar que el receptor y su empleado estén activos
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

            // Validación adicional: verificar que el receptor sea válido según el rol del emisor
            if (usuarioActual.Rol != null &&
                (usuarioActual.Rol.NombreRol.Equals("Abogada", StringComparison.OrdinalIgnoreCase) ||
                 usuarioActual.Rol.NombreRol.Equals("Abogado", StringComparison.OrdinalIgnoreCase)))
            {
                if (UsuarioReceptorSeleccionado.Rol?.NombreRol.Equals("Broker", StringComparison.OrdinalIgnoreCase) == true)
                {
                    MensajeError = "Como Abogada/Abogado no puede enviar tareas al Broker";
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

            // Guardar
            var resultado = await _tareaService.CrearAsync(Tarea);

            if (resultado)
            {
                // Subir archivos adjuntos si hay alguno
                if (ArchivosPendientes.Count > 0)
                {
                    int archivosSubidos = 0;
                    int totalArchivos = ArchivosPendientes.Count;

                    foreach (var archivo in ArchivosPendientes)
                    {
                        var resultadoAdjunto = await _adjuntoService.SubirAdjuntoAsync(
                            Tarea.IdTarea,
                            usuarioActual.IdUsuario,
                            archivo.RutaCompleta);

                        if (resultadoAdjunto)
                        {
                            archivosSubidos++;
                        }
                    }

                    if (archivosSubidos == totalArchivos)
                    {
                        MessageBox.Show($"Tarea creada exitosamente con {archivosSubidos} archivo(s) adjunto(s)",
                                      "Exito",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Tarea creada. Se subieron {archivosSubidos} de {totalArchivos} archivos",
                                      "Advertencia",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Tarea creada exitosamente",
                                  "Exito",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }

                // Notificar al padre que la operación se completó (cerrar panel y recargar lista)
                _onSaveCompleted?.Invoke();
            }
            else
            {
                MensajeError = "Error al crear la tarea";
            }
        }

        private void SeleccionarArchivo()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Todos los archivos (*.*)|*.*|" +
                         "Documentos PDF (*.pdf)|*.pdf|" +
                         "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|" +
                         "Documentos Word (*.doc;*.docx)|*.doc;*.docx|" +
                         "Hojas de cálculo (*.xls;*.xlsx)|*.xls;*.xlsx",
                Title = "Seleccionar archivo para adjuntar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var archivo in openFileDialog.FileNames)
                {
                    var fileInfo = new FileInfo(archivo);

                    // Verificar si el archivo ya está en la lista
                    if (!ArchivosPendientes.Any(a => a.RutaCompleta == archivo))
                    {
                        ArchivosPendientes.Add(new ArchivoPendiente
                        {
                            NombreArchivo = fileInfo.Name,
                            RutaCompleta = archivo,
                            Tamaño = fileInfo.Length,
                            TamañoTexto = FormatearTamaño(fileInfo.Length)
                        });
                    }
                }
            }
        }

        private void EliminarArchivo(ArchivoPendiente archivo)
        {
            ArchivosPendientes.Remove(archivo);
        }

        private string FormatearTamaño(long bytes)
        {
            string[] tamaños = { "B", "KB", "MB", "GB" };
            double tam = bytes;
            int orden = 0;

            while (tam >= 1024 && orden < tamaños.Length - 1)
            {
                orden++;
                tam /= 1024;
            }

            return $"{tam:0.##} {tamaños[orden]}";
        }
    }

    // Clase para representar archivos pendientes de subir
    public class ArchivoPendiente
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaCompleta { get; set; } = string.Empty;
        public long Tamaño { get; set; }
        public string TamañoTexto { get; set; } = string.Empty;
    }
}
