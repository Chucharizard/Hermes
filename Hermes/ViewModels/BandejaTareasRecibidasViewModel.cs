using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class BandejaTareasRecibidasViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly TareaComentarioService _comentarioService;
        private ObservableCollection<Tarea> _tareasRecibidas = new();
        private Tarea? _tareaSeleccionada;
        private string _filtroEstado = "Todas";
        private string _textoBusqueda = string.Empty;
        private ObservableCollection<Tarea> _tareasFiltradas = new();

        public ObservableCollection<Tarea> TareasRecibidas
        {
            get => _tareasRecibidas;
            set => SetProperty(ref _tareasRecibidas, value);
        }

        public ObservableCollection<Tarea> TareasFiltradas
        {
            get => _tareasFiltradas;
            set => SetProperty(ref _tareasFiltradas, value);
        }

        public Tarea? TareaSeleccionada
        {
            get => _tareaSeleccionada;
            set => SetProperty(ref _tareaSeleccionada, value);
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
        public int TotalRecibidas => TareasRecibidas.Count;
        public int TotalPendientes => TareasRecibidas.Count(t => t.EstadoTarea == "Pendiente");
        public int TotalCompletadas => TareasRecibidas.Count(t => t.EstadoTarea == "Completado");
        public int TotalVencidas => TareasRecibidas.Count(t => t.EstadoTarea == "Vencido");
        public int TotalObservadas => TareasRecibidas.Count(t => t.EstadoTarea == "Observado");

        public ICommand RefrescarCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CompletarTareaCommand { get; }
        public ICommand DevolverTareaCommand { get; }

        public BandejaTareasRecibidasViewModel()
        {
            _tareaService = new TareaService();
            _comentarioService = new TareaComentarioService();

            RefrescarCommand = new RelayCommand(async _ => await CargarTareasRecibidasAsync());
            VerDetalleCommand = new RelayCommand(_ => VerDetalleTarea(), _ => TareaSeleccionada != null);
            CompletarTareaCommand = new RelayCommand(async _ => await CompletarTareaAsync(), _ => TareaSeleccionada != null && TareaSeleccionada.EstadoTarea == "Pendiente");
            DevolverTareaCommand = new RelayCommand(_ => DevolverTarea(), _ => TareaSeleccionada != null && TareaSeleccionada.EstadoTarea == "Pendiente");

            Task.Run(async () => await CargarTareasRecibidasAsync());
        }

        private async Task CargarTareasRecibidasAsync()
        {
            var usuarioActual = App.UsuarioActual;
            if (usuarioActual == null)
                return;

            // ⏰ ACTUALIZAR TAREAS VENCIDAS (al estilo Teams)
            // Verifica automáticamente las tareas con fecha límite pasada
            await _tareaService.ActualizarTareasVencidasAsync();

            var tareas = await _tareaService.ObtenerPorReceptorAsync(usuarioActual.IdUsuario);

            Application.Current.Dispatcher.Invoke(() =>
            {
                TareasRecibidas.Clear();
                foreach (var tarea in tareas)
                {
                    TareasRecibidas.Add(tarea);
                }

                AplicarFiltros();
                ActualizarEstadisticas();
            });
        }

        private void AplicarFiltros()
        {
            var tareasFiltradas = TareasRecibidas.AsEnumerable();

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
                    (t.UsuarioEmisor?.Empleado != null &&
                     (t.UsuarioEmisor.Empleado.NombresEmpleado.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                      t.UsuarioEmisor.Empleado.ApellidosEmpleado.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))));
            }

            TareasFiltradas.Clear();
            foreach (var tarea in tareasFiltradas)
            {
                TareasFiltradas.Add(tarea);
            }
        }

        private void ActualizarEstadisticas()
        {
            OnPropertyChanged(nameof(TotalRecibidas));
            OnPropertyChanged(nameof(TotalPendientes));
            OnPropertyChanged(nameof(TotalCompletadas));
            OnPropertyChanged(nameof(TotalVencidas));
            OnPropertyChanged(nameof(TotalObservadas));
        }

        private void VerDetalleTarea()
        {
            if (TareaSeleccionada == null)
                return;

            // Abrir ventana de detalle (se creará más adelante)
            var detalleWindow = new Views.DetalleTareaWindow(TareaSeleccionada);
            if (detalleWindow.ShowDialog() == true)
            {
                // Refrescar lista después de cambios
                Task.Run(async () => await CargarTareasRecibidasAsync());
            }
        }

        private async Task CompletarTareaAsync()
        {
            if (TareaSeleccionada == null || TareaSeleccionada.EstadoTarea != "Pendiente")
                return;

            var resultado = MessageBox.Show(
                $"¿Está seguro de marcar como completada la tarea '{TareaSeleccionada.TituloTarea}'?",
                "Confirmar completado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                TareaSeleccionada.EstadoTarea = "Completado";
                TareaSeleccionada.FechaCompletadaTarea = DateTime.Now;
                var actualizado = await _tareaService.ActualizarAsync(TareaSeleccionada);

                if (actualizado)
                {
                    MessageBox.Show("Tarea marcada como completada",
                                  "Éxito",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    await CargarTareasRecibidasAsync();
                }
                else
                {
                    MessageBox.Show("Error al completar la tarea",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void DevolverTarea()
        {
            if (TareaSeleccionada == null || TareaSeleccionada.EstadoTarea != "Pendiente")
                return;

            // Abrir ventana de detalle para agregar comentario de observación
            // La lógica de devolución se manejará en la ventana de detalle
            VerDetalleTarea();
        }
    }
}
