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
                    var dashboard = workbook.Worksheets.Add("Dashboard");
                    dashboard.TabColor = XLColor.FromHtml("#2E86AB");

                    // Título principal
                    dashboard.Range("B2:I2").Merge();
                    dashboard.Cell("B2").Value = "DASHBOARD DE GESTIÓN DE TAREAS";
                    dashboard.Cell("B2").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(24)
                        .Font.SetFontColor(XLColor.FromHtml("#2D3436"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Subtítulo con fecha
                    dashboard.Range("B3:I3").Merge();
                    dashboard.Cell("B3").Value = $"Reporte generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    dashboard.Cell("B3").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(14)
                        .Font.SetFontColor(XLColor.FromHtml("#2E86AB"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // ========================================
                    // SECCIÓN KPIs PRINCIPALES
                    // ========================================
                    dashboard.Cell("B5").Value = "KPIs PRINCIPALES";
                    dashboard.Cell("B5").Style.Font.SetBold(true).Font.SetFontSize(14);

                    // KPI 1: Total Tareas
                    dashboard.Range("B7:C7").Merge();
                    dashboard.Cell("B7").Value = "TOTAL TAREAS";
                    dashboard.Cell("B7").Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    dashboard.Cell("B7").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    dashboard.Cell("B7").Style.Font.SetFontColor(XLColor.White);

                    dashboard.Range("B8:C9").Merge();
                    dashboard.Cell("B8").Value = totalTareas;
                    dashboard.Cell("B8").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(36)
                        .Font.SetFontColor(XLColor.FromHtml("#2E86AB"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    // KPI 2: Completadas
                    dashboard.Range("D7:E7").Merge();
                    dashboard.Cell("D7").Value = "COMPLETADAS";
                    dashboard.Cell("D7").Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    dashboard.Cell("D7").Style.Fill.BackgroundColor = XLColor.FromHtml("#62C370");
                    dashboard.Cell("D7").Style.Font.SetFontColor(XLColor.White);

                    dashboard.Range("D8:E9").Merge();
                    dashboard.Cell("D8").Value = completadas;
                    dashboard.Cell("D8").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(36)
                        .Font.SetFontColor(XLColor.FromHtml("#62C370"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    dashboard.Range("D10:E10").Merge();
                    dashboard.Cell("D10").Value = totalTareas > 0 ? (double)completadas / totalTareas : 0;
                    dashboard.Cell("D10").Style.NumberFormat.Format = "0.0%";
                    dashboard.Cell("D10").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // KPI 3: Pendientes
                    dashboard.Range("F7:G7").Merge();
                    dashboard.Cell("F7").Value = "PENDIENTES";
                    dashboard.Cell("F7").Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    dashboard.Cell("F7").Style.Fill.BackgroundColor = XLColor.FromHtml("#F18F01");
                    dashboard.Cell("F7").Style.Font.SetFontColor(XLColor.White);

                    dashboard.Range("F8:G9").Merge();
                    dashboard.Cell("F8").Value = pendientes;
                    dashboard.Cell("F8").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(36)
                        .Font.SetFontColor(XLColor.FromHtml("#F18F01"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    dashboard.Range("F10:G10").Merge();
                    dashboard.Cell("F10").Value = totalTareas > 0 ? (double)pendientes / totalTareas : 0;
                    dashboard.Cell("F10").Style.NumberFormat.Format = "0.0%";
                    dashboard.Cell("F10").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // KPI 4: Vencidas
                    dashboard.Range("H7:I7").Merge();
                    dashboard.Cell("H7").Value = "VENCIDAS";
                    dashboard.Cell("H7").Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    dashboard.Cell("H7").Style.Fill.BackgroundColor = XLColor.FromHtml("#C73E1D");
                    dashboard.Cell("H7").Style.Font.SetFontColor(XLColor.White);

                    dashboard.Range("H8:I9").Merge();
                    dashboard.Cell("H8").Value = vencidas;
                    dashboard.Cell("H8").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(36)
                        .Font.SetFontColor(XLColor.FromHtml("#C73E1D"))
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    dashboard.Range("H10:I10").Merge();
                    dashboard.Cell("H10").Value = totalTareas > 0 ? (double)vencidas / totalTareas : 0;
                    dashboard.Cell("H10").Style.NumberFormat.Format = "0.0%";
                    dashboard.Cell("H10").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Agregar bordes a los KPIs
                    dashboard.Range("B7:C10").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    dashboard.Range("D7:E10").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    dashboard.Range("F7:G10").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    dashboard.Range("H7:I10").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

                    // ========================================
                    // TABLA: DISTRIBUCIÓN POR PRIORIDAD
                    // ========================================
                    dashboard.Cell("B13").Value = "DISTRIBUCIÓN POR PRIORIDAD";
                    dashboard.Cell("B13").Style.Font.SetBold(true).Font.SetFontSize(14);

                    // Headers de tabla
                    var headerStyle = dashboard.Cell("B15").Style;
                    headerStyle.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    headerStyle.Font.SetFontColor(XLColor.White);
                    headerStyle.Font.SetBold(true);
                    headerStyle.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    headerStyle.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    dashboard.Cell("B15").Value = "Prioridad";
                    dashboard.Cell("C15").Value = "Cantidad";
                    dashboard.Cell("D15").Value = "Porcentaje";

                    // Aplicar estilo a headers
                    dashboard.Range("B15:D15").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    dashboard.Range("B15:D15").Style.Font.SetFontColor(XLColor.White);
                    dashboard.Range("B15:D15").Style.Font.SetBold(true);
                    dashboard.Range("B15:D15").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Datos de prioridad
                    dashboard.Cell("B16").Value = "Urgente";
                    dashboard.Cell("C16").Value = prioridadUrgente;
                    dashboard.Cell("D16").Value = totalTareas > 0 ? (double)prioridadUrgente / totalTareas : 0;
                    dashboard.Cell("D16").Style.NumberFormat.Format = "0.0%";

                    dashboard.Cell("B17").Value = "Alta";
                    dashboard.Cell("C17").Value = prioridadAlta;
                    dashboard.Cell("D17").Value = totalTareas > 0 ? (double)prioridadAlta / totalTareas : 0;
                    dashboard.Cell("D17").Style.NumberFormat.Format = "0.0%";

                    dashboard.Cell("B18").Value = "Media";
                    dashboard.Cell("C18").Value = prioridadMedia;
                    dashboard.Cell("D18").Value = totalTareas > 0 ? (double)prioridadMedia / totalTareas : 0;
                    dashboard.Cell("D18").Style.NumberFormat.Format = "0.0%";

                    dashboard.Cell("B19").Value = "Baja";
                    dashboard.Cell("C19").Value = prioridadBaja;
                    dashboard.Cell("D19").Value = totalTareas > 0 ? (double)prioridadBaja / totalTareas : 0;
                    dashboard.Cell("D19").Style.NumberFormat.Format = "0.0%";

                    // Total
                    dashboard.Cell("B20").Value = "TOTAL";
                    dashboard.Cell("C20").FormulaA1 = "=SUM(C16:C19)";
                    dashboard.Cell("D20").Value = 1.0;
                    dashboard.Cell("D20").Style.NumberFormat.Format = "0.0%";
                    dashboard.Range("B20:D20").Style.Font.SetBold(true);
                    dashboard.Range("B20:D20").Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");

                    // Agregar bordes a la tabla
                    dashboard.Range("B15:D20").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    dashboard.Range("B15:D20").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // ========================================
                    // TABLA: TOP 3 USUARIOS
                    // ========================================
                    dashboard.Cell("F13").Value = "TOP 3 USUARIOS MÁS PRODUCTIVOS";
                    dashboard.Cell("F13").Style.Font.SetBold(true).Font.SetFontSize(14);

                    // Headers
                    dashboard.Cell("F15").Value = "Ranking";
                    dashboard.Cell("G15").Value = "Usuario";
                    dashboard.Cell("H15").Value = "Asignadas";
                    dashboard.Cell("I15").Value = "Finalizadas";
                    dashboard.Range("F15:I15").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    dashboard.Range("F15:I15").Style.Font.SetFontColor(XLColor.White);
                    dashboard.Range("F15:I15").Style.Font.SetBold(true);
                    dashboard.Range("F15:I15").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Datos Top 3
                    if (estadisticasPorUsuario.Count > 0)
                    {
                        dashboard.Cell("F16").Value = "🥇 1°";
                        dashboard.Cell("G16").Value = estadisticasPorUsuario[0].Usuario;
                        dashboard.Cell("H16").Value = estadisticasPorUsuario[0].TotalTareas;
                        dashboard.Cell("I16").Value = estadisticasPorUsuario[0].TareasFinalizadas;
                    }

                    if (estadisticasPorUsuario.Count > 1)
                    {
                        dashboard.Cell("F17").Value = "🥈 2°";
                        dashboard.Cell("G17").Value = estadisticasPorUsuario[1].Usuario;
                        dashboard.Cell("H17").Value = estadisticasPorUsuario[1].TotalTareas;
                        dashboard.Cell("I17").Value = estadisticasPorUsuario[1].TareasFinalizadas;
                    }

                    if (estadisticasPorUsuario.Count > 2)
                    {
                        dashboard.Cell("F18").Value = "🥉 3°";
                        dashboard.Cell("G18").Value = estadisticasPorUsuario[2].Usuario;
                        dashboard.Cell("H18").Value = estadisticasPorUsuario[2].TotalTareas;
                        dashboard.Cell("I18").Value = estadisticasPorUsuario[2].TareasFinalizadas;
                    }

                    // Agregar bordes
                    dashboard.Range("F15:I18").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    dashboard.Range("F15:I18").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // Ajustar anchos de columna
                    dashboard.Column("A").Width = 2;
                    dashboard.Column("B").Width = 15;
                    dashboard.Column("C").Width = 15;
                    dashboard.Column("D").Width = 15;
                    dashboard.Column("E").Width = 2;
                    dashboard.Column("F").Width = 12;
                    dashboard.Column("G").Width = 25;
                    dashboard.Column("H").Width = 12;
                    dashboard.Column("I").Width = 12;
                    dashboard.Column("J").Width = 2;

                    // ========================================
                    // HOJA 2: DATOS DETALLADOS
                    // ========================================
                    var detailSheet = workbook.Worksheets.Add("Datos Detallados");
                    detailSheet.TabColor = XLColor.FromHtml("#A23B72");

                    // Título
                    detailSheet.Cell("A1").Value = "DATOS DETALLADOS DE TAREAS";
                    detailSheet.Cell("A1").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(18)
                        .Font.SetFontColor(XLColor.FromHtml("#2D3436"));

                    // Sección Estados
                    detailSheet.Cell("A3").Value = "Estados de Tareas";
                    detailSheet.Cell("A3").Style.Font.SetBold(true).Font.SetFontSize(14);

                    detailSheet.Cell("A4").Value = "Estado";
                    detailSheet.Cell("B4").Value = "Cantidad";
                    detailSheet.Cell("C4").Value = "Porcentaje";
                    detailSheet.Range("A4:C4").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    detailSheet.Range("A4:C4").Style.Font.SetFontColor(XLColor.White);
                    detailSheet.Range("A4:C4").Style.Font.SetBold(true);
                    detailSheet.Range("A4:C4").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    detailSheet.Cell("A5").Value = "Pendiente";
                    detailSheet.Cell("B5").Value = pendientes;
                    detailSheet.Cell("C5").FormulaA1 = "=B5/SUM($B$5:$B$8)";
                    detailSheet.Cell("C5").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A6").Value = "Completado";
                    detailSheet.Cell("B6").Value = completadas;
                    detailSheet.Cell("C6").FormulaA1 = "=B6/SUM($B$5:$B$8)";
                    detailSheet.Cell("C6").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A7").Value = "Vencido";
                    detailSheet.Cell("B7").Value = vencidas;
                    detailSheet.Cell("C7").FormulaA1 = "=B7/SUM($B$5:$B$8)";
                    detailSheet.Cell("C7").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A8").Value = "Observado";
                    detailSheet.Cell("B8").Value = observadas;
                    detailSheet.Cell("C8").FormulaA1 = "=B8/SUM($B$5:$B$8)";
                    detailSheet.Cell("C8").Style.NumberFormat.Format = "0.0%";

                    // Total
                    detailSheet.Cell("A9").Value = "TOTAL";
                    detailSheet.Cell("B9").FormulaA1 = "=SUM(B5:B8)";
                    detailSheet.Cell("C9").Value = 1.0;
                    detailSheet.Cell("C9").Style.NumberFormat.Format = "0.0%";
                    detailSheet.Range("A9:C9").Style.Font.SetBold(true);
                    detailSheet.Range("A9:C9").Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");

                    // Bordes
                    detailSheet.Range("A4:C9").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    detailSheet.Range("A4:C9").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // Sección Prioridades
                    detailSheet.Cell("A12").Value = "Prioridades de Tareas";
                    detailSheet.Cell("A12").Style.Font.SetBold(true).Font.SetFontSize(14);

                    detailSheet.Cell("A13").Value = "Prioridad";
                    detailSheet.Cell("B13").Value = "Cantidad";
                    detailSheet.Cell("C13").Value = "Porcentaje";
                    detailSheet.Range("A13:C13").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    detailSheet.Range("A13:C13").Style.Font.SetFontColor(XLColor.White);
                    detailSheet.Range("A13:C13").Style.Font.SetBold(true);
                    detailSheet.Range("A13:C13").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    detailSheet.Cell("A14").Value = "Urgente";
                    detailSheet.Cell("B14").Value = prioridadUrgente;
                    detailSheet.Cell("C14").FormulaA1 = "=B14/SUM($B$14:$B$17)";
                    detailSheet.Cell("C14").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A15").Value = "Alta";
                    detailSheet.Cell("B15").Value = prioridadAlta;
                    detailSheet.Cell("C15").FormulaA1 = "=B15/SUM($B$14:$B$17)";
                    detailSheet.Cell("C15").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A16").Value = "Media";
                    detailSheet.Cell("B16").Value = prioridadMedia;
                    detailSheet.Cell("C16").FormulaA1 = "=B16/SUM($B$14:$B$17)";
                    detailSheet.Cell("C16").Style.NumberFormat.Format = "0.0%";

                    detailSheet.Cell("A17").Value = "Baja";
                    detailSheet.Cell("B17").Value = prioridadBaja;
                    detailSheet.Cell("C17").FormulaA1 = "=B17/SUM($B$14:$B$17)";
                    detailSheet.Cell("C17").Style.NumberFormat.Format = "0.0%";

                    // Total prioridades
                    detailSheet.Cell("A18").Value = "TOTAL";
                    detailSheet.Cell("B18").FormulaA1 = "=SUM(B14:B17)";
                    detailSheet.Cell("C18").Value = 1.0;
                    detailSheet.Cell("C18").Style.NumberFormat.Format = "0.0%";
                    detailSheet.Range("A18:C18").Style.Font.SetBold(true);
                    detailSheet.Range("A18:C18").Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");

                    // Bordes
                    detailSheet.Range("A13:C18").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    detailSheet.Range("A13:C18").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // Ajustar anchos de columna
                    detailSheet.Column("A").Width = 20;
                    detailSheet.Column("B").Width = 15;
                    detailSheet.Column("C").Width = 15;

                    // ========================================
                    // HOJA 3: ANÁLISIS Y MÉTRICAS
                    // ========================================
                    var metricsSheet = workbook.Worksheets.Add("Análisis");
                    metricsSheet.TabColor = XLColor.FromHtml("#62C370");

                    metricsSheet.Cell("A1").Value = "ANÁLISIS Y MÉTRICAS DE RENDIMIENTO";
                    metricsSheet.Cell("A1").Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(18)
                        .Font.SetFontColor(XLColor.FromHtml("#2D3436"));

                    // Métricas clave
                    metricsSheet.Cell("A3").Value = "Métricas Clave";
                    metricsSheet.Cell("A3").Style.Font.SetBold(true).Font.SetFontSize(14);

                    metricsSheet.Cell("A5").Value = "Métrica";
                    metricsSheet.Cell("B5").Value = "Valor";
                    metricsSheet.Cell("C5").Value = "Meta";
                    metricsSheet.Cell("D5").Value = "Estado";
                    metricsSheet.Range("A5:D5").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
                    metricsSheet.Range("A5:D5").Style.Font.SetFontColor(XLColor.White);
                    metricsSheet.Range("A5:D5").Style.Font.SetBold(true);
                    metricsSheet.Range("A5:D5").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Tasa de completitud
                    metricsSheet.Cell("A6").Value = "Tasa de Completitud";
                    var tasaCompletitud = totalTareas > 0 ? (double)completadas / totalTareas : 0;
                    metricsSheet.Cell("B6").Value = tasaCompletitud;
                    metricsSheet.Cell("B6").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("C6").Value = 0.80;
                    metricsSheet.Cell("C6").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("D6").Value = tasaCompletitud >= 0.80 ? "✅ Cumple" : "❌ No cumple";

                    // Tasa de vencimiento
                    metricsSheet.Cell("A7").Value = "Tasa de Vencimiento";
                    var tasaVencimiento = totalTareas > 0 ? (double)vencidas / totalTareas : 0;
                    metricsSheet.Cell("B7").Value = tasaVencimiento;
                    metricsSheet.Cell("B7").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("C7").Value = 0.10;
                    metricsSheet.Cell("C7").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("D7").Value = tasaVencimiento <= 0.10 ? "✅ Cumple" : "❌ No cumple";

                    // Productividad promedio
                    metricsSheet.Cell("A8").Value = "Productividad Promedio Top 3";
                    var productividadPromedio = estadisticasPorUsuario.Any()
                        ? estadisticasPorUsuario.Average(x => x.Eficiencia)
                        : 0;
                    metricsSheet.Cell("B8").Value = productividadPromedio;
                    metricsSheet.Cell("B8").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("C8").Value = 0.70;
                    metricsSheet.Cell("C8").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("D8").Value = productividadPromedio >= 0.70 ? "✅ Cumple" : "❌ No cumple";

                    // Balance de prioridades
                    metricsSheet.Cell("A9").Value = "% Tareas Alta Prioridad";
                    var porcentajeAlta = totalTareas > 0 ? (double)prioridadAlta / totalTareas : 0;
                    metricsSheet.Cell("B9").Value = porcentajeAlta;
                    metricsSheet.Cell("B9").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("C9").Value = 0.30;
                    metricsSheet.Cell("C9").Style.NumberFormat.Format = "0.0%";
                    metricsSheet.Cell("D9").Value = Math.Abs(porcentajeAlta - 0.30) < 0.10 ? "✅ Balanceado" : "⚠️ Revisar";

                    // Aplicar bordes
                    metricsSheet.Range("A5:D9").Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                    metricsSheet.Range("A5:D9").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // Ajustar anchos
                    metricsSheet.Column("A").Width = 30;
                    metricsSheet.Column("B").Width = 15;
                    metricsSheet.Column("C").Width = 15;
                    metricsSheet.Column("D").Width = 20;

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
