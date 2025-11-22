using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class UsuarioStats
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public int TareasCompletadas { get; set; }
    }

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
        private ObservableCollection<Tarea> _tareasUrgentes;
        private ObservableCollection<Tarea> _actividadReciente;
        private ObservableCollection<UsuarioStats> _top3Usuarios;
        private double _tasaCompletado;
        private double _tasaTiempo;
        private double _eficienciaGeneral;
        private double _tiempoPromedioCompletado;
        private double _tareasPorUsuario;
        private double _productividadSemanal;

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

        public ObservableCollection<Tarea> TareasUrgentes
        {
            get => _tareasUrgentes;
            set => SetProperty(ref _tareasUrgentes, value);
        }

        public ObservableCollection<Tarea> ActividadReciente
        {
            get => _actividadReciente;
            set => SetProperty(ref _actividadReciente, value);
        }

        public ObservableCollection<UsuarioStats> Top3Usuarios
        {
            get => _top3Usuarios;
            set => SetProperty(ref _top3Usuarios, value);
        }

        public double TasaCompletado
        {
            get => _tasaCompletado;
            set => SetProperty(ref _tasaCompletado, value);
        }

        public double TasaTiempo
        {
            get => _tasaTiempo;
            set => SetProperty(ref _tasaTiempo, value);
        }

        public double EficienciaGeneral
        {
            get => _eficienciaGeneral;
            set => SetProperty(ref _eficienciaGeneral, value);
        }

        public double TiempoPromedioCompletado
        {
            get => _tiempoPromedioCompletado;
            set => SetProperty(ref _tiempoPromedioCompletado, value);
        }

        public double TareasPorUsuario
        {
            get => _tareasPorUsuario;
            set => SetProperty(ref _tareasPorUsuario, value);
        }

        public double ProductividadSemanal
        {
            get => _productividadSemanal;
            set => SetProperty(ref _productividadSemanal, value);
        }

        public ICommand GenerarDashboardConsolidadoCommand { get; }
        public ICommand ActualizarEstadisticasCommand { get; }

        public DashboardViewModel()
        {
            _tareaService = new TareaService();
            _excelService = new ExcelService();

            TareasUrgentes = new ObservableCollection<Tarea>();
            ActividadReciente = new ObservableCollection<Tarea>();
            Top3Usuarios = new ObservableCollection<UsuarioStats>();

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

            // Filtrar tareas urgentes (Alta prioridad, no completadas)
            var tareasUrgentes = tareas
                .Where(t => (t.PrioridadTarea == "Alta" || t.PrioridadTarea == "Urgente") &&
                            t.EstadoTarea != "Completado" &&
                            t.EstadoTarea != "Archivada")
                .OrderBy(t => t.FechaLimiteTarea)
                .Take(5)
                .ToList();

            // Obtener actividad reciente (últimas 5 tareas por fecha de inicio)
            var actividadReciente = tareas
                .OrderByDescending(t => t.FechaInicioTarea)
                .Take(5)
                .ToList();

            // Calcular Top 3 Usuarios por tareas completadas
            var usuariosStats = tareas
                .Where(t => !string.IsNullOrWhiteSpace(t.Receptor))
                .GroupBy(t => t.Receptor)
                .Select(g => new UsuarioStats
                {
                    NombreUsuario = g.Key,
                    TareasCompletadas = g.Count(t => t.EstadoTarea == "Completado")
                })
                .OrderByDescending(u => u.TareasCompletadas)
                .Take(3)
                .ToList();

            // Asegurar que siempre haya 3 usuarios (con valores por defecto si no hay suficientes)
            while (usuariosStats.Count < 3)
            {
                usuariosStats.Add(new UsuarioStats
                {
                    NombreUsuario = $"Usuario {usuariosStats.Count + 1}",
                    TareasCompletadas = 0
                });
            }

            // Calcular indicadores de progreso
            var totalTareasCount = tareas.Count > 0 ? tareas.Count : 1;
            var tasaCompletadoCalc = (tareas.Count(t => t.EstadoTarea == "Completado") / (double)totalTareasCount) * 100;

            var tareasConFecha = tareas.Where(t => t.FechaLimiteTarea.HasValue && t.EstadoTarea == "Completado").ToList();
            var tareasATiempo = tareasConFecha.Count(t => t.FechaFinTarea.HasValue && t.FechaFinTarea <= t.FechaLimiteTarea);
            var tasaTiempoCalc = tareasConFecha.Count > 0 ? (tareasATiempo / (double)tareasConFecha.Count) * 100 : 0;

            var eficienciaGeneralCalc = (tasaCompletadoCalc + tasaTiempoCalc) / 2;

            // Calcular mini estadísticas
            var tareasCompletadasConFechas = tareas
                .Where(t => t.EstadoTarea == "Completado" && t.FechaInicioTarea.HasValue && t.FechaFinTarea.HasValue)
                .ToList();

            var tiempoPromedioCalc = tareasCompletadasConFechas.Count > 0
                ? tareasCompletadasConFechas.Average(t => (t.FechaFinTarea!.Value - t.FechaInicioTarea!.Value).TotalDays)
                : 0;

            var totalUsuarios = tareas.Where(t => !string.IsNullOrWhiteSpace(t.Receptor)).Select(t => t.Receptor).Distinct().Count();
            var tareasPorUsuarioCalc = totalUsuarios > 0 ? tareas.Count / (double)totalUsuarios : 0;

            // Productividad semanal: tareas completadas en los últimos 7 días
            var tareasUltimaSemana = tareas.Count(t =>
                t.EstadoTarea == "Completado" &&
                t.FechaFinTarea.HasValue &&
                t.FechaFinTarea >= DateTime.Now.AddDays(-7));
            var productividadSemanalCalc = tareasUltimaSemana;

            Application.Current.Dispatcher.Invoke(() =>
            {
                TotalTareas = tareas.Count;
                TareasPendientes = tareas.Count(t => t.EstadoTarea == "Pendiente");
                TareasCompletadas = tareas.Count(t => t.EstadoTarea == "Completado");
                TareasVencidas = tareas.Count(t => t.EstadoTarea == "Vencido");
                TareasObservadas = tareas.Count(t => t.EstadoTarea == "Observado");

                // Actualizar estadísticas de widgets
                TasaCompletado = tasaCompletadoCalc;
                TasaTiempo = tasaTiempoCalc;
                EficienciaGeneral = eficienciaGeneralCalc;
                TiempoPromedioCompletado = tiempoPromedioCalc;
                TareasPorUsuario = tareasPorUsuarioCalc;
                ProductividadSemanal = productividadSemanalCalc;

                // Actualizar colecciones
                TareasUrgentes.Clear();
                foreach (var tarea in tareasUrgentes)
                    TareasUrgentes.Add(tarea);

                ActividadReciente.Clear();
                foreach (var tarea in actividadReciente)
                    ActividadReciente.Add(tarea);

                Top3Usuarios.Clear();
                foreach (var usuario in usuariosStats)
                    Top3Usuarios.Add(usuario);
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
