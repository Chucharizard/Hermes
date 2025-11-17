using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class DetalleTareaViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly TareaComentarioService _comentarioService;
        private readonly TareaAdjuntoService _adjuntoService;
        private readonly UsuarioService _usuarioService;
        private Tarea _tarea;
        private string _nuevoComentario = string.Empty;
        private ObservableCollection<TareaComentario> _comentarios = new();
        private ObservableCollection<TareaAdjunto> _adjuntos = new();
        private ObservableCollection<TareaAdjunto> _adjuntosEmisor = new();
        private ObservableCollection<TareaAdjunto> _adjuntosReceptor = new();
        private ObservableCollection<Usuario> _usuariosReceptores = new();
        private Usuario? _nuevoReceptorSeleccionado;

        public Tarea Tarea
        {
            get => _tarea;
            set => SetProperty(ref _tarea, value);
        }

        public string NuevoComentario
        {
            get => _nuevoComentario;
            set => SetProperty(ref _nuevoComentario, value);
        }

        public ObservableCollection<TareaComentario> Comentarios
        {
            get => _comentarios;
            set => SetProperty(ref _comentarios, value);
        }

        public ObservableCollection<TareaAdjunto> Adjuntos
        {
            get => _adjuntos;
            set => SetProperty(ref _adjuntos, value);
        }

        public ObservableCollection<TareaAdjunto> AdjuntosEmisor
        {
            get => _adjuntosEmisor;
            set => SetProperty(ref _adjuntosEmisor, value);
        }

        public ObservableCollection<TareaAdjunto> AdjuntosReceptor
        {
            get => _adjuntosReceptor;
            set => SetProperty(ref _adjuntosReceptor, value);
        }

        public ObservableCollection<Usuario> UsuariosReceptores
        {
            get => _usuariosReceptores;
            set => SetProperty(ref _usuariosReceptores, value);
        }

        public Usuario? NuevoReceptorSeleccionado
        {
            get => _nuevoReceptorSeleccionado;
            set => SetProperty(ref _nuevoReceptorSeleccionado, value);
        }

        // Propiedades calculadas para la UI
        public bool EsEmisor => App.UsuarioActual?.IdUsuario == Tarea?.UsuarioEmisorId;
        public bool EsReceptor => App.UsuarioActual?.IdUsuario == Tarea?.UsuarioReceptorId;
        public bool PuedeCompletar => EsReceptor && Tarea?.EstadoTarea == "Pendiente";
        public bool PuedeDevolver => EsReceptor && Tarea?.EstadoTarea == "Pendiente";
        public bool PuedeArchivar => EsEmisor && Tarea?.EstadoTarea == "Completado";
        public bool PuedeReasignar => EsEmisor && Tarea?.EstadoTarea == "Observado";
        public bool PuedeObservar => EsEmisor && Tarea?.EstadoTarea == "Completado";
        public int TotalComentarios => Comentarios.Count;
        public int TotalAdjuntos => Adjuntos.Count;
        public int TotalAdjuntosEmisor => AdjuntosEmisor.Count;
        public int TotalAdjuntosReceptor => AdjuntosReceptor.Count;

        public ICommand AgregarComentarioCommand { get; }
        public ICommand SubirAdjuntoCommand { get; }
        public ICommand DescargarAdjuntoCommand { get; }
        public ICommand EliminarAdjuntoCommand { get; }
        public ICommand CompletarTareaCommand { get; }
        public ICommand DevolverTareaCommand { get; }
        public ICommand ObservarTareaCommand { get; }
        public ICommand ArchivarTareaCommand { get; }
        public ICommand ReasignarTareaCommand { get; }
        public ICommand CerrarCommand { get; }

        public DetalleTareaViewModel(Tarea tarea)
        {
            _tareaService = new TareaService();
            _comentarioService = new TareaComentarioService();
            _adjuntoService = new TareaAdjuntoService();
            _usuarioService = new UsuarioService();

            _tarea = tarea;

            AgregarComentarioCommand = new RelayCommand(async _ => await AgregarComentarioAsync(), _ => !string.IsNullOrWhiteSpace(NuevoComentario));
            SubirAdjuntoCommand = new RelayCommand(async _ => await SubirAdjuntoAsync());
            DescargarAdjuntoCommand = new RelayCommand(async param => await DescargarAdjuntoAsync((TareaAdjunto)param!));
            EliminarAdjuntoCommand = new RelayCommand(async param => await EliminarAdjuntoAsync((TareaAdjunto)param!));
            CompletarTareaCommand = new RelayCommand(async _ => await CompletarTareaAsync(), _ => PuedeCompletar);
            DevolverTareaCommand = new RelayCommand(async _ => await DevolverTareaAsync(), _ => PuedeDevolver);
            ObservarTareaCommand = new RelayCommand(async _ => await ObservarTareaAsync(), _ => PuedeObservar);
            ArchivarTareaCommand = new RelayCommand(async _ => await ArchivarTareaAsync(), _ => PuedeArchivar);
            ReasignarTareaCommand = new RelayCommand(async _ => await ReasignarTareaAsync(), _ => PuedeReasignar && NuevoReceptorSeleccionado != null);
            CerrarCommand = new RelayCommand(_ => Cerrar());

            Task.Run(async () => await CargarDatosAsync());
        }

        private async Task CargarDatosAsync()
        {
            // Cargar comentarios
            var comentarios = await _comentarioService.ObtenerComentariosPorTareaAsync(Tarea.IdTarea);

            // Cargar adjuntos
            var adjuntos = await _adjuntoService.ObtenerAdjuntosPorTareaAsync(Tarea.IdTarea);

            // Cargar usuarios para reasignación (si es necesario)
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Comentarios.Clear();
                foreach (var comentario in comentarios)
                {
                    Comentarios.Add(comentario);
                }

                Adjuntos.Clear();
                AdjuntosEmisor.Clear();
                AdjuntosReceptor.Clear();

                foreach (var adjunto in adjuntos)
                {
                    Adjuntos.Add(adjunto);

                    // Separar por usuario que subió
                    if (adjunto.IdUsuarioSubioTareaAdjunto == Tarea.UsuarioEmisorId)
                    {
                        AdjuntosEmisor.Add(adjunto);
                    }
                    else if (adjunto.IdUsuarioSubioTareaAdjunto == Tarea.UsuarioReceptorId)
                    {
                        AdjuntosReceptor.Add(adjunto);
                    }
                }

                // Cargar usuarios receptores (excluyendo al emisor)
                UsuariosReceptores.Clear();
                foreach (var usuario in usuarios.Where(u => u.EsActivoUsuario && u.IdUsuario != Tarea.UsuarioEmisorId))
                {
                    UsuariosReceptores.Add(usuario);
                }

                ActualizarPropiedades();
            });
        }

        private async Task AgregarComentarioAsync()
        {
            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null || !usuarioActual.EsActivoUsuario ||
                usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MessageBox.Show("Su usuario o empleado está inactivo. No puede agregar comentarios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NuevoComentario))
                return;

            var comentario = new TareaComentario
            {
                IdTarea = Tarea.IdTarea,
                IdUsuario = App.UsuarioActual!.IdUsuario,
                Comentario = NuevoComentario,
                FechaComentario = DateTime.Now
            };

            var resultado = await _comentarioService.AgregarComentarioAsync(comentario);

            if (resultado)
            {
                NuevoComentario = string.Empty;
                await CargarDatosAsync();
            }
            else
            {
                MessageBox.Show("Error al agregar comentario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SubirAdjuntoAsync()
        {
            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null || !usuarioActual.EsActivoUsuario ||
                usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MessageBox.Show("Su usuario o empleado está inactivo. No puede subir adjuntos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Todos los archivos (*.*)|*.*",
                Title = "Seleccionar archivo para adjuntar"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var resultado = await _adjuntoService.SubirAdjuntoAsync(
                    Tarea.IdTarea,
                    App.UsuarioActual!.IdUsuario,
                    openFileDialog.FileName);

                if (resultado)
                {
                    MessageBox.Show("Archivo adjuntado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarDatosAsync();
                }
                else
                {
                    MessageBox.Show("Error al adjuntar archivo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DescargarAdjuntoAsync(TareaAdjunto adjunto)
        {
            var rutaArchivo = await _adjuntoService.ObtenerRutaArchivoAsync(adjunto.IdAdjunto);

            if (rutaArchivo != null && File.Exists(rutaArchivo))
            {
                try
                {
                    // Abrir archivo con la aplicación predeterminada
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rutaArchivo,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Archivo no encontrado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EliminarAdjuntoAsync(TareaAdjunto adjunto)
        {
            // Verificar que el usuario actual sea quien subió el archivo
            if (adjunto.IdUsuarioSubioTareaAdjunto != App.UsuarioActual?.IdUsuario)
            {
                MessageBox.Show("Solo puedes eliminar los archivos que tú subiste", "Permiso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro de eliminar el archivo '{adjunto.NombreArchivo}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                var eliminado = await _adjuntoService.EliminarAsync(adjunto.IdAdjunto);

                if (eliminado)
                {
                    MessageBox.Show("Archivo eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarDatosAsync();
                }
                else
                {
                    MessageBox.Show("Error al eliminar archivo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task CompletarTareaAsync()
        {
            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null || !usuarioActual.EsActivoUsuario ||
                usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MessageBox.Show("Su usuario o empleado está inactivo. No puede completar tareas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!PuedeCompletar)
                return;

            var resultado = MessageBox.Show(
                "¿Está seguro de marcar esta tarea como completada?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                Tarea.EstadoTarea = "Completado";
                Tarea.FechaCompletadaTarea = DateTime.Now;

                var actualizado = await _tareaService.ActualizarAsync(Tarea);

                if (actualizado)
                {
                    MessageBox.Show("Tarea completada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActualizarPropiedades();
                }
            }
        }

        private async Task DevolverTareaAsync()
        {
            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null || !usuarioActual.EsActivoUsuario ||
                usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MessageBox.Show("Su usuario o empleado está inactivo. No puede devolver tareas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!PuedeDevolver || string.IsNullOrWhiteSpace(NuevoComentario))
            {
                MessageBox.Show("Debe agregar un comentario explicando por qué devuelve la tarea", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Agregar comentario de observación
            await AgregarComentarioAsync();

            // Cambiar estado a Observado
            Tarea.EstadoTarea = "Observado";
            var actualizado = await _tareaService.ActualizarAsync(Tarea);

            if (actualizado)
            {
                MessageBox.Show("Tarea devuelta al emisor con observaciones", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ActualizarPropiedades();
            }
        }

        private async Task ObservarTareaAsync()
        {
            // VALIDACIÓN CRÍTICA: Verificar que el usuario actual esté activo
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null || !usuarioActual.EsActivoUsuario ||
                usuarioActual.Empleado == null || !usuarioActual.Empleado.EsActivoEmpleado)
            {
                MessageBox.Show("Su usuario o empleado está inactivo. No puede observar tareas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!PuedeObservar || string.IsNullOrWhiteSpace(NuevoComentario))
            {
                MessageBox.Show("Debe agregar un comentario explicando las observaciones", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Agregar comentario de observación
            await AgregarComentarioAsync();

            // Cambiar estado a Observado
            Tarea.EstadoTarea = "Observado";
            var actualizado = await _tareaService.ActualizarAsync(Tarea);

            if (actualizado)
            {
                MessageBox.Show("Tarea devuelta al receptor con observaciones", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ActualizarPropiedades();
            }
        }

        private async Task ArchivarTareaAsync()
        {
            if (!PuedeArchivar)
                return;

            var resultado = MessageBox.Show(
                "¿Está seguro de archivar esta tarea?",
                "Confirmar archivado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                Tarea.EstadoTarea = "Archivado";
                var actualizado = await _tareaService.ActualizarAsync(Tarea);

                if (actualizado)
                {
                    MessageBox.Show("Tarea archivada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarConDialogResult(true);
                }
            }
        }

        private async Task ReasignarTareaAsync()
        {
            if (!PuedeReasignar || NuevoReceptorSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un nuevo receptor para reasignar la tarea", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro de reasignar esta tarea a {NuevoReceptorSeleccionado.Empleado?.NombresEmpleado} {NuevoReceptorSeleccionado.Empleado?.ApellidosEmpleado}?",
                "Confirmar reasignación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                Tarea.UsuarioReceptorId = NuevoReceptorSeleccionado.IdUsuario;
                Tarea.EstadoTarea = "Pendiente";
                var actualizado = await _tareaService.ActualizarAsync(Tarea);

                if (actualizado)
                {
                    MessageBox.Show("Tarea reasignada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarConDialogResult(true);
                }
            }
        }

        private void ActualizarPropiedades()
        {
            OnPropertyChanged(nameof(EsEmisor));
            OnPropertyChanged(nameof(EsReceptor));
            OnPropertyChanged(nameof(PuedeCompletar));
            OnPropertyChanged(nameof(PuedeDevolver));
            OnPropertyChanged(nameof(PuedeArchivar));
            OnPropertyChanged(nameof(PuedeReasignar));
            OnPropertyChanged(nameof(PuedeObservar));
            OnPropertyChanged(nameof(TotalComentarios));
            OnPropertyChanged(nameof(TotalAdjuntos));
            OnPropertyChanged(nameof(TotalAdjuntosEmisor));
            OnPropertyChanged(nameof(TotalAdjuntosReceptor));
        }

        private void Cerrar()
        {
            CerrarConDialogResult(false);
        }

        private void CerrarConDialogResult(bool dialogResult)
        {
            var ventana = Application.Current.Windows.OfType<Views.DetalleTareaWindow>().FirstOrDefault();
            if (ventana != null)
            {
                ventana.DialogResult = dialogResult;
                ventana.Close();
            }
        }
    }
}
