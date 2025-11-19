using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Helpers;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class GestionTareasViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private ObservableCollection<Tarea> _tareas = new();
        private ObservableCollection<Tarea> _tareasFiltradas = new();
        private Tarea? _tareaSeleccionada;
        private DetalleTareaViewModel? _detalleViewModel;
        private string _filtroTexto = string.Empty;

        public ObservableCollection<Tarea> Tareas
        {
            get => _tareas;
            set => SetProperty(ref _tareas, value);
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

        public string FiltroTexto
        {
            get => _filtroTexto;
            set
            {
                SetProperty(ref _filtroTexto, value);
                FiltrarTareas();
            }
        }

        public int TotalTareas => Tareas?.Count ?? 0;
        public int TareasPendientes => Tareas?.Count(t => t.EstadoTarea == "Pendiente") ?? 0;
        public int TareasCompletadas => Tareas?.Count(t => t.EstadoTarea == "Completado") ?? 0;
        public int TareasVencidas => Tareas?.Count(t => t.EstadoTarea == "Vencido") ?? 0;

        public ICommand CargarTareasCommand { get; }
        public ICommand AbrirNuevaTareaCommand { get; }
        public ICommand EditarTareaCommand { get; }
        public ICommand EliminarTareaCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public GestionTareasViewModel()
        {
            _tareaService = new TareaService();
            Tareas = new ObservableCollection<Tarea>();
            TareasFiltradas = new ObservableCollection<Tarea>();

            CargarTareasCommand = new RelayCommand(async _ => await CargarTareasAsync());
            AbrirNuevaTareaCommand = new RelayCommand(_ => AbrirNuevaTarea());
            EditarTareaCommand = new RelayCommand(tarea => EditarTarea(tarea as Tarea));
            EliminarTareaCommand = new RelayCommand(tarea => EliminarTarea(tarea as Tarea));
            VerDetalleCommand = new RelayCommand(tarea => VerDetalle(tarea as Tarea));

            // Cargar datos iniciales
            Task.Run(async () => await CargarTareasAsync());
        }

        private async Task CargarTareasAsync()
        {
            // ⏰ ACTUALIZAR TAREAS VENCIDAS (al estilo Teams)
            await _tareaService.ActualizarTareasVencidasAsync();

            var tareas = await _tareaService.ObtenerTodasAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tareas.Clear();
                foreach (var tarea in tareas)
                {
                    Tareas.Add(tarea);
                }

                FiltrarTareas();
                OnPropertyChanged(nameof(TotalTareas));
                OnPropertyChanged(nameof(TareasPendientes));
                OnPropertyChanged(nameof(TareasCompletadas));
                OnPropertyChanged(nameof(TareasVencidas));
            });
        }

        private void FiltrarTareas()
        {
            TareasFiltradas.Clear();

            var filtradas = string.IsNullOrWhiteSpace(FiltroTexto)
                ? Tareas
                : Tareas.Where(t =>
                    t.TituloTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    (t.DescripcionTarea?.ToLower().Contains(FiltroTexto.ToLower()) ?? false) ||
                    t.EstadoTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    t.PrioridadTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    (t.UsuarioEmisor?.Empleado?.NombresEmpleado.ToLower().Contains(FiltroTexto.ToLower()) ?? false) ||
                    (t.UsuarioReceptor?.Empleado?.NombresEmpleado.ToLower().Contains(FiltroTexto.ToLower()) ?? false));

            foreach (var tarea in filtradas)
            {
                TareasFiltradas.Add(tarea);
            }
        }

        private void AbrirNuevaTarea()
        {
            var ventana = new Views.NuevaTareaWindow();
            ventana.Owner = Application.Current.MainWindow;
            if (ventana.ShowDialog() == true)
            {
                Task.Run(async () => await CargarTareasAsync());
            }
        }

        private void EditarTarea(Tarea? tarea)
        {
            if (tarea == null) return;

            var ventana = new Views.EditarTareaWindow(tarea);
            ventana.Owner = Application.Current.MainWindow;
            if (ventana.ShowDialog() == true)
            {
                Task.Run(async () => await CargarTareasAsync());
            }
        }

        private async void EliminarTarea(Tarea? tarea)
        {
            if (tarea == null) return;

            // Usar DialogHelper para confirmación consistente
            if (!DialogHelper.ConfirmarEliminacion(tarea.TituloTarea))
                return;

            try
            {
                var eliminado = await _tareaService.EliminarAsync(tarea.IdTarea);

                if (eliminado)
                {
                    DialogHelper.MostrarExito("La tarea ha sido eliminada exitosamente.");
                    await CargarTareasAsync();
                }
                else
                {
                    DialogHelper.MostrarError("No se pudo eliminar la tarea. Por favor, intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                DialogHelper.MostrarError($"Error al eliminar la tarea:\n{ex.Message}");
            }
        }

        private void VerDetalle(Tarea? tarea)
        {
            if (tarea == null) return;

            var emisor = tarea.UsuarioEmisor?.Empleado != null
                ? $"{tarea.UsuarioEmisor.Empleado.NombresEmpleado} {tarea.UsuarioEmisor.Empleado.ApellidosEmpleado}"
                : "No especificado";

            var receptor = tarea.UsuarioReceptor?.Empleado != null
                ? $"{tarea.UsuarioReceptor.Empleado.NombresEmpleado} {tarea.UsuarioReceptor.Empleado.ApellidosEmpleado}"
                : "No especificado";

            var fechaLimite = tarea.FechaLimiteTarea.HasValue
                ? tarea.FechaLimiteTarea.Value.ToString("dd/MM/yyyy")
                : "Sin fecha limite";

            var fechaCompletada = tarea.FechaCompletadaTarea.HasValue
                ? tarea.FechaCompletadaTarea.Value.ToString("dd/MM/yyyy HH:mm")
                : "No completada";

            MessageBox.Show(
                $"DETALLE DE TAREA\n\n" +
                $"Titulo: {tarea.TituloTarea}\n" +
                $"Descripcion: {tarea.DescripcionTarea ?? "Sin descripcion"}\n\n" +
                $"Estado: {tarea.EstadoTarea}\n" +
                $"Prioridad: {tarea.PrioridadTarea}\n\n" +
                $"Emisor: {emisor}\n" +
                $"Receptor: {receptor}\n\n" +
                $"Fecha Inicio: {tarea.FechaInicioTarea:dd/MM/yyyy}\n" +
                $"Fecha Limite: {fechaLimite}\n" +
                $"Fecha Completada: {fechaCompletada}",
                "Informacion de la Tarea",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
