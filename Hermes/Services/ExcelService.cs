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

                // Barra visual de progreso (usando celdas coloreadas)
                worksheetEstadisticas.Cell(2, 5).Value = "Visualización";
                worksheetEstadisticas.Cell(2, 5).Style.Font.Bold = true;
                worksheetEstadisticas.Cell(2, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#3498DB");
                worksheetEstadisticas.Cell(2, 5).Style.Font.FontColor = XLColor.White;

                // Crear barras visuales usando el ancho de la celda
                int maxBarWidth = 30;

                // Barra Pendientes
                int widthPendientes = totalTareas > 0 ? (int)((double)pendientes / totalTareas * maxBarWidth) : 0;
                var barraPendientes = new string('█', widthPendientes);
                worksheetEstadisticas.Cell(3, 5).Value = barraPendientes;
                worksheetEstadisticas.Cell(3, 5).Style.Font.FontColor = XLColor.FromHtml("#F39C12");

                // Barra Completadas
                int widthCompletadas = totalTareas > 0 ? (int)((double)completadas / totalTareas * maxBarWidth) : 0;
                var barraCompletadas = new string('█', widthCompletadas);
                worksheetEstadisticas.Cell(4, 5).Value = barraCompletadas;
                worksheetEstadisticas.Cell(4, 5).Style.Font.FontColor = XLColor.FromHtml("#27AE60");

                // Barra Vencidas
                int widthVencidas = totalTareas > 0 ? (int)((double)vencidas / totalTareas * maxBarWidth) : 0;
                var barraVencidas = new string('█', widthVencidas);
                worksheetEstadisticas.Cell(5, 5).Value = barraVencidas;
                worksheetEstadisticas.Cell(5, 5).Style.Font.FontColor = XLColor.FromHtml("#E74C3C");

                // Barra Observadas
                int widthObservadas = totalTareas > 0 ? (int)((double)observadas / totalTareas * maxBarWidth) : 0;
                var barraObservadas = new string('█', widthObservadas);
                worksheetEstadisticas.Cell(6, 5).Value = barraObservadas;
                worksheetEstadisticas.Cell(6, 5).Style.Font.FontColor = XLColor.FromHtml("#9B59B6");

                // Instrucciones para crear gráfico
                worksheetEstadisticas.Cell(9, 2).Value = "💡 CÓMO CREAR GRÁFICO:";
                worksheetEstadisticas.Cell(9, 2).Style.Font.Bold = true;
                worksheetEstadisticas.Cell(9, 2).Style.Font.FontSize = 12;

                worksheetEstadisticas.Cell(10, 2).Value = "1. Selecciona el rango B2:C6";
                worksheetEstadisticas.Cell(11, 2).Value = "2. Ve a 'Insertar' > 'Gráfico'";
                worksheetEstadisticas.Cell(12, 2).Value = "3. Elige 'Columnas' o 'Barras'";
                worksheetEstadisticas.Cell(13, 2).Value = "4. ¡Listo! Tendrás tu gráfico profesional";

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

                // Barra visual de progreso para prioridades
                worksheetPrioridades.Cell(2, 5).Value = "Visualización";
                worksheetPrioridades.Cell(2, 5).Style.Font.Bold = true;
                worksheetPrioridades.Cell(2, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#9B59B6");
                worksheetPrioridades.Cell(2, 5).Style.Font.FontColor = XLColor.White;

                // Barras visuales
                int widthBaja = totalTareas > 0 ? (int)((double)bajas / totalTareas * maxBarWidth) : 0;
                worksheetPrioridades.Cell(3, 5).Value = new string('█', widthBaja);
                worksheetPrioridades.Cell(3, 5).Style.Font.FontColor = XLColor.FromHtml("#95A5A6");

                int widthMedia = totalTareas > 0 ? (int)((double)medias / totalTareas * maxBarWidth) : 0;
                worksheetPrioridades.Cell(4, 5).Value = new string('█', widthMedia);
                worksheetPrioridades.Cell(4, 5).Style.Font.FontColor = XLColor.FromHtml("#3498DB");

                int widthAlta = totalTareas > 0 ? (int)((double)altas / totalTareas * maxBarWidth) : 0;
                worksheetPrioridades.Cell(5, 5).Value = new string('█', widthAlta);
                worksheetPrioridades.Cell(5, 5).Style.Font.FontColor = XLColor.FromHtml("#F39C12");

                int widthUrgente = totalTareas > 0 ? (int)((double)urgentes / totalTareas * maxBarWidth) : 0;
                worksheetPrioridades.Cell(6, 5).Value = new string('█', widthUrgente);
                worksheetPrioridades.Cell(6, 5).Style.Font.FontColor = XLColor.FromHtml("#E74C3C");

                // Instrucciones para crear gráfico
                worksheetPrioridades.Cell(8, 2).Value = "💡 CÓMO CREAR GRÁFICO DE TORTA:";
                worksheetPrioridades.Cell(8, 2).Style.Font.Bold = true;
                worksheetPrioridades.Cell(8, 2).Style.Font.FontSize = 12;

                worksheetPrioridades.Cell(9, 2).Value = "1. Selecciona el rango B2:C6";
                worksheetPrioridades.Cell(10, 2).Value = "2. Ve a 'Insertar' > 'Gráfico'";
                worksheetPrioridades.Cell(11, 2).Value = "3. Elige 'Circular' (Pie Chart)";
                worksheetPrioridades.Cell(12, 2).Value = "4. ¡Listo! Verás la distribución de prioridades";

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
