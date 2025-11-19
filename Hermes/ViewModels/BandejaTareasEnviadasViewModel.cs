using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class BandejaTareasEnviadasViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly TareaComentarioService _comentarioService;
        private ObservableCollection<Tarea> _tareasEnviadas = new();
        private Tarea? _tareaSeleccionada;
        private DetalleTareaViewModel? _detalleViewModel;
        private string _filtroEstado = "Todas";
        private string _textoBusqueda = string.Empty;
        private ObservableCollection<Tarea> _tareasFiltradas = new();

        public ObservableCollection<Tarea> TareasEnviadas
        {
            get => _tareasEnviadas;
            set => SetProperty(ref _tareasEnviadas, value);
        }

        public ObservableCollection<Tarea> TareasFiltradas
        {
            get => _tareasFiltradas;
            set => SetProperty(ref _tareasFiltradas, value);
        }

        public Tarea? TareaSeleccionada
        {
            get => _tareaSeleccionada;
            set
            {
                if (SetProperty(ref _tareaSeleccionada, value))
                {
                    // Crear ViewModel de detalle cuando se selecciona una tarea
                    DetalleViewModel = value != null ? new DetalleTareaViewModel(value) : null;
                }
            }
        }

        public DetalleTareaViewModel? DetalleViewModel
        {
            get => _detalleViewModel;
            set => SetProperty(ref _detalleViewModel, value);
        }

        public string FiltroEstado
        {
            get => _filtroEstado;
            set
            {
                SetProperty(ref _filtroEstado, value);
                AplicarFiltros();
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                SetProperty(ref _textoBusqueda, value);
                AplicarFiltros();
            }
        }

        public List<string> EstadosFiltro { get; } = new()
        {
            "Todas",
            "Pendiente",
            "Completado",
            "Vencido",
            "Observado",
            "Archivado"
        };

        // Estadísticas
        public int TotalEnviadas => TareasEnviadas.Count;
        public int TotalPendientes => TareasEnviadas.Count(t => t.EstadoTarea == "Pendiente");
        public int TotalCompletadas => TareasEnviadas.Count(t => t.EstadoTarea == "Completado");
        public int TotalObservadas => TareasEnviadas.Count(t => t.EstadoTarea == "Observado");
        public int TotalArchivadas => TareasEnviadas.Count(t => t.EstadoTarea == "Archivado");

        public ICommand RefrescarCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand ArchivarTareaCommand { get; }
        public ICommand AbrirNuevaTareaCommand { get; }
        public ICommand VolverCommand { get; }

        public BandejaTareasEnviadasViewModel()
        {
            _tareaService = new TareaService();
            _comentarioService = new TareaComentarioService();

            RefrescarCommand = new RelayCommand(async _ => await CargarTareasEnviadasAsync());
            VerDetalleCommand = new RelayCommand(_ => VerDetalleTarea(), _ => TareaSeleccionada != null);
            ArchivarTareaCommand = new RelayCommand(async _ => await ArchivarTareaAsync(), _ => TareaSeleccionada != null && TareaSeleccionada.EstadoTarea == "Completado");
            AbrirNuevaTareaCommand = new RelayCommand(_ => AbrirNuevaTarea());
            VolverCommand = new RelayCommand(_ => Volver());

            Task.Run(async () => await CargarTareasEnviadasAsync());
        }

        private async Task CargarTareasEnviadasAsync()
        {
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null)
                return;

            // ⏰ ACTUALIZAR TAREAS VENCIDAS (al estilo Teams)
            await _tareaService.ActualizarTareasVencidasAsync();

            var tareas = await _tareaService.ObtenerPorEmisorAsync(usuarioActual.IdUsuario);

            Application.Current.Dispatcher.Invoke(() =>
            {
                TareasEnviadas.Clear();
                foreach (var tarea in tareas)
                {
                    TareasEnviadas.Add(tarea);
                }

                AplicarFiltros();
                ActualizarEstadisticas();
            });
        }

        private void AplicarFiltros()
        {
            var tareasFiltradas = TareasEnviadas.AsEnumerable();

            // Filtrar por estado
            if (FiltroEstado != "Todas")
            {
                tareasFiltradas = tareasFiltradas.Where(t => t.EstadoTarea == FiltroEstado);
            }

            // Filtrar por búsqueda
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                tareasFiltradas = tareasFiltradas.Where(t =>
                    t.TituloTarea.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                    (t.DescripcionTarea?.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.UsuarioReceptor?.Empleado != null &&
                     (t.UsuarioReceptor.Empleado.NombresEmpleado.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                      t.UsuarioReceptor.Empleado.ApellidosEmpleado.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))));
            }

            TareasFiltradas.Clear();
            foreach (var tarea in tareasFiltradas)
            {
                TareasFiltradas.Add(tarea);
            }
        }

        private void ActualizarEstadisticas()
        {
            OnPropertyChanged(nameof(TotalEnviadas));
            OnPropertyChanged(nameof(TotalPendientes));
            OnPropertyChanged(nameof(TotalCompletadas));
            OnPropertyChanged(nameof(TotalObservadas));
            OnPropertyChanged(nameof(TotalArchivadas));
        }

        private void VerDetalleTarea()
        {
            // Este método ya no es necesario porque el detalle se muestra inline
            // La selección de la tarea ya activa el DetalleViewModel automáticamente
            // Se mantiene para compatibilidad con el comando
        }

        private async Task ArchivarTareaAsync()
        {
            if (TareaSeleccionada == null || TareaSeleccionada.EstadoTarea != "Completado")
                return;

            var resultado = MessageBox.Show(
                $"¿Está seguro de archivar la tarea '{TareaSeleccionada.TituloTarea}'?",
                "Confirmar archivado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                TareaSeleccionada.EstadoTarea = "Archivado";
                var actualizado = await _tareaService.ActualizarAsync(TareaSeleccionada);

                if (actualizado)
                {
                    MessageBox.Show("Tarea archivada exitosamente",
                                  "Éxito",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    await CargarTareasEnviadasAsync();
                }
                else
                {
                    MessageBox.Show("Error al archivar la tarea",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void AbrirNuevaTarea()
        {
            var nuevaTareaWindow = new Views.NuevaTareaWindow();
            if (nuevaTareaWindow.ShowDialog() == true)
            {
                // Refrescar lista después de crear la tarea
                Task.Run(async () => await CargarTareasEnviadasAsync());
            }
        }

        private void Volver()
        {
            // Limpiar selección para volver a la vista de lista
            TareaSeleccionada = null;
        }
    }
}
