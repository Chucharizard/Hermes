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

        public async Task<string> GenerarReporteTareasAsync()
        {
            // Obtener todas las tareas
            var tareas = await _tareaService.ObtenerTodasAsync();

            // Crear archivo Excel
            using (var workbook = new XLWorkbook())
            {
                // ===== HOJA 1: DATOS DE TAREAS =====
                var worksheetDatos = workbook.Worksheets.Add("Datos de Tareas");

                // Headers
                worksheetDatos.Cell(1, 1).Value = "Título";
                worksheetDatos.Cell(1, 2).Value = "Descripción";
                worksheetDatos.Cell(1, 3).Value = "Estado";
                worksheetDatos.Cell(1, 4).Value = "Prioridad";
                worksheetDatos.Cell(1, 5).Value = "Emisor";
                worksheetDatos.Cell(1, 6).Value = "Receptor";
                worksheetDatos.Cell(1, 7).Value = "Fecha Inicio";
                worksheetDatos.Cell(1, 8).Value = "Fecha Límite";
                worksheetDatos.Cell(1, 9).Value = "Fecha Completada";

                // Estilo del header
                var headerRange = worksheetDatos.Range("A1:I1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var tarea in tareas)
                {
                    worksheetDatos.Cell(row, 1).Value = tarea.TituloTarea;
                    worksheetDatos.Cell(row, 2).Value = tarea.DescripcionTarea ?? "";
                    worksheetDatos.Cell(row, 3).Value = tarea.EstadoTarea;
                    worksheetDatos.Cell(row, 4).Value = tarea.PrioridadTarea;

                    var emisor = tarea.UsuarioEmisor?.Empleado != null
                        ? $"{tarea.UsuarioEmisor.Empleado.NombresEmpleado} {tarea.UsuarioEmisor.Empleado.ApellidosEmpleado}"
                        : "N/A";
                    worksheetDatos.Cell(row, 5).Value = emisor;

                    var receptor = tarea.UsuarioReceptor?.Empleado != null
                        ? $"{tarea.UsuarioReceptor.Empleado.NombresEmpleado} {tarea.UsuarioReceptor.Empleado.ApellidosEmpleado}"
                        : "N/A";
                    worksheetDatos.Cell(row, 6).Value = receptor;

                    worksheetDatos.Cell(row, 7).Value = tarea.FechaInicioTarea.ToString("dd/MM/yyyy");
                    worksheetDatos.Cell(row, 8).Value = tarea.FechaLimiteTarea?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheetDatos.Cell(row, 9).Value = tarea.FechaCompletadaTarea?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                    row++;
                }

                // Ajustar ancho de columnas
                worksheetDatos.Columns().AdjustToContents();

                // ===== HOJA 2: ESTADÍSTICAS POR ESTADO =====
                var worksheetEstadisticas = workbook.Worksheets.Add("Estadísticas");

                // Calcular estadísticas
                var totalTareas = tareas.Count;
                var pendientes = tareas.Count(t => t.EstadoTarea == "Pendiente");
                var completadas = tareas.Count(t => t.EstadoTarea == "Completado");
                var vencidas = tareas.Count(t => t.EstadoTarea == "Vencido");
                var observadas = tareas.Count(t => t.EstadoTarea == "Observado");

                // Headers estadísticas
                worksheetEstadisticas.Cell(2, 2).Value = "Estado";
                worksheetEstadisticas.Cell(2, 3).Value = "Cantidad";
                worksheetEstadisticas.Cell(2, 4).Value = "Porcentaje";

                var headerStats = worksheetEstadisticas.Range("B2:D2");
                headerStats.Style.Font.Bold = true;
                headerStats.Style.Fill.BackgroundColor = XLColor.FromHtml("#3498DB");
                headerStats.Style.Font.FontColor = XLColor.White;
                headerStats.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos estadísticas
                worksheetEstadisticas.Cell(3, 2).Value = "Pendiente";
                worksheetEstadisticas.Cell(3, 3).Value = pendientes;
                worksheetEstadisticas.Cell(3, 4).Value = totalTareas > 0 ? (double)pendientes / totalTareas : 0;
                worksheetEstadisticas.Cell(3, 4).Style.NumberFormat.Format = "0.00%";
                worksheetEstadisticas.Range("B3:D3").Style.Fill.BackgroundColor = XLColor.FromHtml("#F39C12");

                worksheetEstadisticas.Cell(4, 2).Value = "Completado";
                worksheetEstadisticas.Cell(4, 3).Value = completadas;
                worksheetEstadisticas.Cell(4, 4).Value = totalTareas > 0 ? (double)completadas / totalTareas : 0;
                worksheetEstadisticas.Cell(4, 4).Style.NumberFormat.Format = "0.00%";
                worksheetEstadisticas.Range("B4:D4").Style.Fill.BackgroundColor = XLColor.FromHtml("#27AE60");

                worksheetEstadisticas.Cell(5, 2).Value = "Vencido";
                worksheetEstadisticas.Cell(5, 3).Value = vencidas;
                worksheetEstadisticas.Cell(5, 4).Value = totalTareas > 0 ? (double)vencidas / totalTareas : 0;
                worksheetEstadisticas.Cell(5, 4).Style.NumberFormat.Format = "0.00%";
                worksheetEstadisticas.Range("B5:D5").Style.Fill.BackgroundColor = XLColor.FromHtml("#E74C3C");

                worksheetEstadisticas.Cell(6, 2).Value = "Observado";
                worksheetEstadisticas.Cell(6, 3).Value = observadas;
                worksheetEstadisticas.Cell(6, 4).Value = totalTareas > 0 ? (double)observadas / totalTareas : 0;
                worksheetEstadisticas.Cell(6, 4).Style.NumberFormat.Format = "0.00%";
                worksheetEstadisticas.Range("B6:D6").Style.Fill.BackgroundColor = XLColor.FromHtml("#9B59B6");

                // Total
                worksheetEstadisticas.Cell(7, 2).Value = "TOTAL";
                worksheetEstadisticas.Cell(7, 3).Value = totalTareas;
                worksheetEstadisticas.Cell(7, 4).Value = 1.0;
                worksheetEstadisticas.Cell(7, 4).Style.NumberFormat.Format = "0.00%";
                var totalRange = worksheetEstadisticas.Range("B7:D7");
                totalRange.Style.Font.Bold = true;
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                totalRange.Style.Font.FontColor = XLColor.White;

                // Título
                worksheetEstadisticas.Cell(1, 2).Value = "ESTADÍSTICAS DE TAREAS POR ESTADO";
                worksheetEstadisticas.Range("B1:D1").Merge();
                worksheetEstadisticas.Cell(1, 2).Style.Font.Bold = true;
                worksheetEstadisticas.Cell(1, 2).Style.Font.FontSize = 16;
                worksheetEstadisticas.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // GRÁFICO DE BARRAS
                var chart = worksheetEstadisticas.AddChart("Tareas por Estado", XLChartType.ColumnClustered);
                chart.SetPosition(10, 0, 2, 0);
                chart.SetSize(600, 400);

                var datosGrafico = worksheetEstadisticas.Range("B3:C6");
                chart.AddDataSource(datosGrafico);

                // Ajustar columnas
                worksheetEstadisticas.Columns().AdjustToContents();

                // ===== HOJA 3: PRIORIDADES =====
                var worksheetPrioridades = workbook.Worksheets.Add("Prioridades");

                var bajas = tareas.Count(t => t.PrioridadTarea == "Baja");
                var medias = tareas.Count(t => t.PrioridadTarea == "Media");
                var altas = tareas.Count(t => t.PrioridadTarea == "Alta");
                var urgentes = tareas.Count(t => t.PrioridadTarea == "Urgente");

                worksheetPrioridades.Cell(1, 2).Value = "DISTRIBUCIÓN POR PRIORIDAD";
                worksheetPrioridades.Range("B1:D1").Merge();
                worksheetPrioridades.Cell(1, 2).Style.Font.Bold = true;
                worksheetPrioridades.Cell(1, 2).Style.Font.FontSize = 16;
                worksheetPrioridades.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheetPrioridades.Cell(2, 2).Value = "Prioridad";
                worksheetPrioridades.Cell(2, 3).Value = "Cantidad";
                worksheetPrioridades.Cell(2, 4).Value = "Porcentaje";

                var headerPrio = worksheetPrioridades.Range("B2:D2");
                headerPrio.Style.Font.Bold = true;
                headerPrio.Style.Fill.BackgroundColor = XLColor.FromHtml("#9B59B6");
                headerPrio.Style.Font.FontColor = XLColor.White;

                worksheetPrioridades.Cell(3, 2).Value = "Baja";
                worksheetPrioridades.Cell(3, 3).Value = bajas;
                worksheetPrioridades.Cell(3, 4).Value = totalTareas > 0 ? (double)bajas / totalTareas : 0;
                worksheetPrioridades.Cell(3, 4).Style.NumberFormat.Format = "0.00%";

                worksheetPrioridades.Cell(4, 2).Value = "Media";
                worksheetPrioridades.Cell(4, 3).Value = medias;
                worksheetPrioridades.Cell(4, 4).Value = totalTareas > 0 ? (double)medias / totalTareas : 0;
                worksheetPrioridades.Cell(4, 4).Style.NumberFormat.Format = "0.00%";

                worksheetPrioridades.Cell(5, 2).Value = "Alta";
                worksheetPrioridades.Cell(5, 3).Value = altas;
                worksheetPrioridades.Cell(5, 4).Value = totalTareas > 0 ? (double)altas / totalTareas : 0;
                worksheetPrioridades.Cell(5, 4).Style.NumberFormat.Format = "0.00%";

                worksheetPrioridades.Cell(6, 2).Value = "Urgente";
                worksheetPrioridades.Cell(6, 3).Value = urgentes;
                worksheetPrioridades.Cell(6, 4).Value = totalTareas > 0 ? (double)urgentes / totalTareas : 0;
                worksheetPrioridades.Cell(6, 4).Style.NumberFormat.Format = "0.00%";

                // Gráfico de torta
                var chartPie = worksheetPrioridades.AddChart("Prioridades", XLChartType.Pie);
                chartPie.SetPosition(8, 0, 2, 0);
                chartPie.SetSize(600, 400);

                var datosPrioridad = worksheetPrioridades.Range("B3:C6");
                chartPie.AddDataSource(datosPrioridad);

                worksheetPrioridades.Columns().AdjustToContents();

                // Guardar archivo
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var fileName = $"Reporte_Tareas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(desktopPath, fileName);

                workbook.SaveAs(filePath);

                return filePath;
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
