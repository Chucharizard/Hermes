using ClosedXML.Excel;
using Hermes.Models;
using System.Diagnostics;
using System.IO;

namespace Hermes.Services
{
    public class ExcelService
    {
        private readonly TareaService _tareaService;

        public ExcelService()
        {
            _tareaService = new TareaService();
        }

        public async Task<string> GenerarReporteDashboardConsolidadoAsync()
        {
            try
            {
                // Obtener todas las tareas
                var tareas = await _tareaService.ObtenerTodasAsync();

                // Calcular estadísticas generales
                var totalTareas = tareas.Count;
                var pendientes = tareas.Count(t => t.EstadoTarea == "Pendiente");
                var completadas = tareas.Count(t => t.EstadoTarea == "Completado");
                var vencidas = tareas.Count(t => t.EstadoTarea == "Vencido");
                var observadas = tareas.Count(t => t.EstadoTarea == "Observado");

                // Calcular estadísticas por prioridad
                var prioridadUrgente = tareas.Count(t => t.PrioridadTarea == "Urgente");
                var prioridadAlta = tareas.Count(t => t.PrioridadTarea == "Alta");
                var prioridadMedia = tareas.Count(t => t.PrioridadTarea == "Media");
                var prioridadBaja = tareas.Count(t => t.PrioridadTarea == "Baja");

                // Calcular Top 3 usuarios
                var estadisticasPorUsuario = tareas
                    .Where(t => t.UsuarioReceptor != null && t.UsuarioReceptor.Empleado != null)
                    .GroupBy(t => new
                    {
                        UsuarioId = t.UsuarioReceptorId,
                        Nombre = $"{t.UsuarioReceptor.Empleado.NombresEmpleado} {t.UsuarioReceptor.Empleado.ApellidosEmpleado}"
                    })
                    .Select(g => new
                    {
                        Usuario = g.Key.Nombre,
                        TotalTareas = g.Count(),
                        TareasFinalizadas = g.Count(t => t.EstadoTarea == "Completado")
                    })
                    .OrderByDescending(x => x.TareasFinalizadas)
                    .Take(3)
                    .ToList();

                // Ruta de la plantilla
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var templatePath = Path.Combine(baseDirectory, "Resources", "Plantillas", "Reporte_Dashboard_Consolidado.xlsx");

                // Verificar que existe la plantilla
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException(
                        $"No se encontró la plantilla de Excel en: {templatePath}\n\n" +
                        "Por favor, asegúrate de que el archivo 'Reporte_Dashboard_Consolidado.xlsx' esté en la carpeta 'Resources/Plantillas' del proyecto.");
                }

                // Crear ruta de destino en el escritorio
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var fileName = $"Dashboard_Consolidado_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(desktopPath, fileName);

                // Copiar la plantilla al escritorio
                File.Copy(templatePath, filePath, true);

                // Abrir el archivo copiado y actualizar solo las celdas de datos
                using (var workbook = new XLWorkbook(filePath))
                {
                    // ========================================
                    // HOJA 1: DASHBOARD PRINCIPAL
                    // ========================================
                    var dashboard = workbook.Worksheet(1);

                    // Actualizar solo los VALORES de las celdas (la plantilla ya tiene el formato)

                    // Fecha del reporte
                    dashboard.Cell("B3").Value = $"Reporte generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                    // KPIs principales
                    dashboard.Cell("B8").Value = totalTareas;
                    dashboard.Cell("D8").Value = completadas;
                    dashboard.Cell("D10").Value = totalTareas > 0 ? (double)completadas / totalTareas : 0;
                    dashboard.Cell("F8").Value = pendientes;
                    dashboard.Cell("F10").Value = totalTareas > 0 ? (double)pendientes / totalTareas : 0;
                    dashboard.Cell("H8").Value = vencidas;
                    dashboard.Cell("H10").Value = totalTareas > 0 ? (double)vencidas / totalTareas : 0;

                    // Distribución por prioridad (la plantilla tiene: Alta, Media, Baja)
                    dashboard.Cell("C16").Value = prioridadAlta;
                    dashboard.Cell("D16").Value = totalTareas > 0 ? (double)prioridadAlta / totalTareas : 0;
                    dashboard.Cell("C17").Value = prioridadMedia;
                    dashboard.Cell("D17").Value = totalTareas > 0 ? (double)prioridadMedia / totalTareas : 0;
                    dashboard.Cell("C18").Value = prioridadBaja;
                    dashboard.Cell("D18").Value = totalTareas > 0 ? (double)prioridadBaja / totalTareas : 0;

                    // Top 3 usuarios
                    if (estadisticasPorUsuario.Count > 0)
                    {
                        dashboard.Cell("G16").Value = estadisticasPorUsuario[0].Usuario;
                        dashboard.Cell("H16").Value = estadisticasPorUsuario[0].TotalTareas;
                        dashboard.Cell("I16").Value = estadisticasPorUsuario[0].TareasFinalizadas;
                    }

                    if (estadisticasPorUsuario.Count > 1)
                    {
                        dashboard.Cell("G17").Value = estadisticasPorUsuario[1].Usuario;
                        dashboard.Cell("H17").Value = estadisticasPorUsuario[1].TotalTareas;
                        dashboard.Cell("I17").Value = estadisticasPorUsuario[1].TareasFinalizadas;
                    }

                    if (estadisticasPorUsuario.Count > 2)
                    {
                        dashboard.Cell("G18").Value = estadisticasPorUsuario[2].Usuario;
                        dashboard.Cell("H18").Value = estadisticasPorUsuario[2].TotalTareas;
                        dashboard.Cell("I18").Value = estadisticasPorUsuario[2].TareasFinalizadas;
                    }

                    // ========================================
                    // HOJA 2: DATOS DETALLADOS
                    // ========================================
                    var detailSheet = workbook.Worksheet(2);

                    // Estados de tareas
                    detailSheet.Cell("B5").Value = pendientes;
                    detailSheet.Cell("B6").Value = completadas;
                    detailSheet.Cell("B7").Value = vencidas;
                    detailSheet.Cell("B8").Value = observadas;

                    // Prioridades (la plantilla tiene: Alta, Media, Baja)
                    detailSheet.Cell("B14").Value = prioridadAlta;
                    detailSheet.Cell("B15").Value = prioridadMedia;
                    detailSheet.Cell("B16").Value = prioridadBaja;

                    // ========================================
                    // HOJA 3: ANÁLISIS Y MÉTRICAS
                    // ========================================
                    var metricsSheet = workbook.Worksheet(3);

                    // Métricas de rendimiento
                    var tasaCompletitud = totalTareas > 0 ? (double)completadas / totalTareas : 0;
                    metricsSheet.Cell("B6").Value = tasaCompletitud;
                    metricsSheet.Cell("D6").Value = tasaCompletitud >= 0.80 ? "✅ Cumple" : "❌ No cumple";

                    var tasaVencimiento = totalTareas > 0 ? (double)vencidas / totalTareas : 0;
                    metricsSheet.Cell("B7").Value = tasaVencimiento;
                    metricsSheet.Cell("D7").Value = tasaVencimiento <= 0.10 ? "✅ Cumple" : "❌ No cumple";

                    var productividadPromedio = estadisticasPorUsuario.Any()
                        ? estadisticasPorUsuario.Average(x => x.TotalTareas > 0 ? (double)x.TareasFinalizadas / x.TotalTareas : 0)
                        : 0;
                    metricsSheet.Cell("B8").Value = productividadPromedio;
                    metricsSheet.Cell("D8").Value = productividadPromedio >= 0.70 ? "✅ Cumple" : "❌ No cumple";

                    var porcentajeAlta = totalTareas > 0 ? (double)prioridadAlta / totalTareas : 0;
                    metricsSheet.Cell("B9").Value = porcentajeAlta;
                    metricsSheet.Cell("D9").Value = Math.Abs(porcentajeAlta - 0.30) < 0.10 ? "✅ Balanceado" : "⚠️ Revisar";

                    // Guardar el archivo
                    workbook.SaveAs(filePath);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el dashboard consolidado: {ex.Message}", ex);
            }
        }

        public void AbrirArchivo(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
    }
}
