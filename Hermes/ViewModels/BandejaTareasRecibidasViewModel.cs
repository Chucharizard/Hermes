using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class BandejaTareasRecibidasViewModel : BaseViewModel, IDropTarget
    {
        private readonly TareaService _tareaService;
        private readonly TareaComentarioService _comentarioService;
        private ObservableCollection<Tarea> _tareasRecibidas = new();
        private Tarea? _tareaSeleccionada;
        private DetalleTareaViewModel? _detalleViewModel;
        private string _filtroEstado = "Todas";
        private string _textoBusqueda = string.Empty;
        private ObservableCollection<Tarea> _tareasFiltradas = new();

        // Colecciones Kanban por estado
        private ObservableCollection<Tarea> _tareasPendientes = new();
        private ObservableCollection<Tarea> _tareasCompletadas = new();
        private ObservableCollection<Tarea> _tareasVencidas = new();
        private ObservableCollection<Tarea> _tareasObservadas = new();

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

        // Colecciones Kanban
        public ObservableCollection<Tarea> TareasPendientes
        {
            get => _tareasPendientes;
            set => SetProperty(ref _tareasPendientes, value);
        }

        public ObservableCollection<Tarea> TareasCompletadas
        {
            get => _tareasCompletadas;
            set => SetProperty(ref _tareasCompletadas, value);
        }

        public ObservableCollection<Tarea> TareasVencidas
        {
            get => _tareasVencidas;
            set => SetProperty(ref _tareasVencidas, value);
        }

        public ObservableCollection<Tarea> TareasObservadas
        {
            get => _tareasObservadas;
            set => SetProperty(ref _tareasObservadas, value);
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
        public int TotalRecibidas => TareasRecibidas.Count;
        public int TotalPendientes => TareasRecibidas.Count(t => t.EstadoTarea == "Pendiente");
        public int TotalCompletadas => TareasRecibidas.Count(t => t.EstadoTarea == "Completado");
        public int TotalVencidas => TareasRecibidas.Count(t => t.EstadoTarea == "Vencido");
        public int TotalObservadas => TareasRecibidas.Count(t => t.EstadoTarea == "Observado");

        public ICommand RefrescarCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CompletarTareaCommand { get; }
        public ICommand DevolverTareaCommand { get; }
        public ICommand VolverCommand { get; }

        public BandejaTareasRecibidasViewModel()
        {
            _tareaService = new TareaService();
            _comentarioService = new TareaComentarioService();

            RefrescarCommand = new RelayCommand(async _ => await CargarTareasRecibidasAsync());
            VerDetalleCommand = new RelayCommand(_ => VerDetalleTarea(), _ => TareaSeleccionada != null);
            CompletarTareaCommand = new RelayCommand(async _ => await CompletarTareaAsync(), _ => TareaSeleccionada != null && TareaSeleccionada.EstadoTarea == "Pendiente");
            DevolverTareaCommand = new RelayCommand(_ => DevolverTarea(), _ => TareaSeleccionada != null && TareaSeleccionada.EstadoTarea == "Pendiente");
            VolverCommand = new RelayCommand(_ => Volver());

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
                OrganizarTareasKanban();
                ActualizarEstadisticas();
            });
        }

        private void OrganizarTareasKanban()
        {
            TareasPendientes.Clear();
            TareasCompletadas.Clear();
            TareasVencidas.Clear();
            TareasObservadas.Clear();

            var tareasAOrganizar = string.IsNullOrWhiteSpace(TextoBusqueda)
                ? TareasRecibidas
                : TareasFiltradas;

            foreach (var tarea in tareasAOrganizar)
            {
                switch (tarea.EstadoTarea)
                {
                    case "Pendiente":
                        TareasPendientes.Add(tarea);
                        break;
                    case "Completado":
                        TareasCompletadas.Add(tarea);
                        break;
                    case "Vencido":
                        TareasVencidas.Add(tarea);
                        break;
                    case "Observado":
                        TareasObservadas.Add(tarea);
                        break;
                }
            }
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

            // Reorganizar Kanban después de aplicar filtros
            OrganizarTareasKanban();
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
            // Este método ya no es necesario porque el detalle se muestra inline
            // La selección de la tarea ya activa el DetalleViewModel automáticamente
            // Se mantiene para compatibilidad con el comando
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

        private void Volver()
        {
            // Limpiar selección para volver a la vista de lista
            TareaSeleccionada = null;
        }

        // Implementación de IDropTarget para drag & drop
        public void DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as Tarea;
            var targetCollection = dropInfo.TargetCollection;

            if (sourceItem != null && targetCollection != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            var tarea = dropInfo.Data as Tarea;
            var targetCollection = dropInfo.TargetCollection as ObservableCollection<Tarea>;

            if (tarea == null || targetCollection == null)
                return;

            // Determinar el nuevo estado basado en la colección de destino
            string nuevoEstado = DeterminarEstadoPorColeccion(targetCollection);

            if (nuevoEstado == tarea.EstadoTarea)
                return; // No hacer nada si es el mismo estado

            // Actualizar estado de la tarea
            var estadoAnterior = tarea.EstadoTarea;
            tarea.EstadoTarea = nuevoEstado;

            // Si se completa, guardar fecha de completado
            if (nuevoEstado == "Completado" && estadoAnterior != "Completado")
            {
                tarea.FechaCompletadaTarea = DateTime.Now;
            }

            // Actualizar en base de datos
            var actualizado = await _tareaService.ActualizarAsync(tarea);

            if (actualizado)
            {
                // Reorganizar las colecciones Kanban
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OrganizarTareasKanban();
                    ActualizarEstadisticas();
                });
            }
            else
            {
                // Revertir cambio si falla
                tarea.EstadoTarea = estadoAnterior;
                MessageBox.Show("Error al actualizar el estado de la tarea",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private string DeterminarEstadoPorColeccion(ObservableCollection<Tarea> coleccion)
        {
            if (coleccion == TareasPendientes) return "Pendiente";
            if (coleccion == TareasCompletadas) return "Completado";
            if (coleccion == TareasVencidas) return "Vencido";
            if (coleccion == TareasObservadas) return "Observado";
            return "Pendiente"; // Default
        }
    }
}
