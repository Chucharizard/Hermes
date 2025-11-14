using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly ExcelService _excelService;
        private int _totalTareas;
        private int _tareasPendientes;
        private int _tareasCompletadas;
        private int _tareasVencidas;
        private int _tareasObservadas;
        private bool _generandoReporte;

        public int TotalTareas
        {
            get => _totalTareas;
            set => SetProperty(ref _totalTareas, value);
        }

        public int TareasPendientes
        {
            get => _tareasPendientes;
            set => SetProperty(ref _tareasPendientes, value);
        }

        public int TareasCompletadas
        {
            get => _tareasCompletadas;
            set => SetProperty(ref _tareasCompletadas, value);
        }

        public int TareasVencidas
        {
            get => _tareasVencidas;
            set => SetProperty(ref _tareasVencidas, value);
        }

        public int TareasObservadas
        {
            get => _tareasObservadas;
            set => SetProperty(ref _tareasObservadas, value);
        }

        public bool GenerandoReporte
        {
            get => _generandoReporte;
            set => SetProperty(ref _generandoReporte, value);
        }

        public ICommand GenerarReporteExcelCommand { get; }
        public ICommand ActualizarEstadisticasCommand { get; }

        public DashboardViewModel()
        {
            _tareaService = new TareaService();
            _excelService = new ExcelService();

            GenerarReporteExcelCommand = new RelayCommand(async _ => await GenerarReporteExcelAsync());
            ActualizarEstadisticasCommand = new RelayCommand(async _ => await CargarEstadisticasAsync());

            // Cargar estadísticas iniciales
            Task.Run(async () => await CargarEstadisticasAsync());
        }

        private async Task CargarEstadisticasAsync()
        {
            var tareas = await _tareaService.ObtenerTodasAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                TotalTareas = tareas.Count;
                TareasPendientes = tareas.Count(t => t.EstadoTarea == "Pendiente");
                TareasCompletadas = tareas.Count(t => t.EstadoTarea == "Completado");
                TareasVencidas = tareas.Count(t => t.EstadoTarea == "Vencido");
                TareasObservadas = tareas.Count(t => t.EstadoTarea == "Observado");
            });
        }

        private async Task GenerarReporteExcelAsync()
        {
            try
            {
                GenerandoReporte = true;

                var filePath = await _excelService.GenerarReporteTareasAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Reporte generado exitosamente!\n\nArchivo guardado en:\n{filePath}",
                        "Reporte Excel",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Abrir el archivo automáticamente
                    _excelService.AbrirArchivo(filePath);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Error al generar el reporte:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                GenerandoReporte = false;
            }
        }
    }
}
