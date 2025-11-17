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

        public ICommand GenerarDashboardConsolidadoCommand { get; }
        public ICommand ActualizarEstadisticasCommand { get; }

        public DashboardViewModel()
        {
            _tareaService = new TareaService();
            _excelService = new ExcelService();

            GenerarDashboardConsolidadoCommand = new RelayCommand(async _ => await GenerarDashboardConsolidadoAsync());
            ActualizarEstadisticasCommand = new RelayCommand(async _ => await CargarEstadisticasAsync());

            // Cargar estadísticas iniciales
            Task.Run(async () => await CargarEstadisticasAsync());
        }

        private async Task CargarEstadisticasAsync()
        {
            // ⏰ ACTUALIZAR TAREAS VENCIDAS (al estilo Teams)
            await _tareaService.ActualizarTareasVencidasAsync();

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

        private async Task GenerarDashboardConsolidadoAsync()
        {
            try
            {
                GenerandoReporte = true;

                var filePath = await _excelService.GenerarReporteDashboardConsolidadoAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Dashboard consolidado generado exitosamente!\n\n" +
                        $"El reporte incluye:\n" +
                        $"• Hoja 1: Dashboard principal con KPIs\n" +
                        $"• Hoja 2: Datos detallados y estadísticas\n" +
                        $"• Hoja 3: Análisis y métricas de rendimiento\n\n" +
                        $"Archivo guardado en:\n{filePath}",
                        "Dashboard Consolidado",
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
                        $"Error al generar el dashboard consolidado:\n{ex.Message}",
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
