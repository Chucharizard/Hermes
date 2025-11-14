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

            // Calcular estadísticas
            var pendientes = tareas.Count(t => t.EstadoTarea == "Pendiente");
            var completadas = tareas.Count(t => t.EstadoTarea == "Completado");
            var vencidas = tareas.Count(t => t.EstadoTarea == "Vencido");

            // Ruta de la plantilla
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDirectory, "Resources", "Plantillas", "Tareas.xlsx");

            // Verificar que existe la plantilla
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    $"No se encontró la plantilla de Excel en: {templatePath}\n\n" +
                    "Por favor, asegúrate de que el archivo 'Tareas.xlsx' esté en la carpeta 'Resources/Plantillas' del proyecto.");
            }

            // Crear ruta de destino en el escritorio
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileName = $"Reporte_Tareas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(desktopPath, fileName);

            // Copiar la plantilla al escritorio
            File.Copy(templatePath, filePath, true);

            // Abrir el archivo copiado y actualizar solo las celdas de datos
            using (var workbook = new XLWorkbook(filePath))
            {
                // Obtener la primera hoja (asumiendo que la plantilla tiene los datos aquí)
                var worksheet = workbook.Worksheet(1);

                // Actualizar las celdas con los conteos
                // B2 = PENDIENTE
                worksheet.Cell("B2").Value = pendientes;

                // B3 = COMPLETADA
                worksheet.Cell("B3").Value = completadas;

                // B4 = VENCIDA
                worksheet.Cell("B4").Value = vencidas;

                // Guardar los cambios (los gráficos se actualizarán automáticamente)
                workbook.Save();
            }

            return filePath;
        }

        public async Task<string> GenerarReportePrioridadesAsync()
        {
            // Obtener todas las tareas
            var tareas = await _tareaService.ObtenerTodasAsync();

            // Calcular estadísticas por prioridad
            var altas = tareas.Count(t => t.PrioridadTarea == "Alta");
            var medias = tareas.Count(t => t.PrioridadTarea == "Media");
            var bajas = tareas.Count(t => t.PrioridadTarea == "Baja");

            // Ruta de la plantilla
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDirectory, "Resources", "Plantillas", "Prioridades.xlsx");

            // Verificar que existe la plantilla
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    $"No se encontró la plantilla de Excel en: {templatePath}\n\n" +
                    "Por favor, asegúrate de que el archivo 'Prioridades.xlsx' esté en la carpeta 'Resources/Plantillas' del proyecto.");
            }

            // Crear ruta de destino en el escritorio
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileName = $"Reporte_Prioridades_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(desktopPath, fileName);

            // Copiar la plantilla al escritorio
            File.Copy(templatePath, filePath, true);

            // Abrir el archivo copiado y actualizar solo las celdas de datos
            using (var workbook = new XLWorkbook(filePath))
            {
                // Obtener la primera hoja
                var worksheet = workbook.Worksheet(1);

                // Actualizar las celdas con los conteos
                // B2 = ALTA
                worksheet.Cell("B2").Value = altas;

                // B3 = MEDIA
                worksheet.Cell("B3").Value = medias;

                // B4 = BAJA
                worksheet.Cell("B4").Value = bajas;

                // Guardar los cambios (los gráficos se actualizarán automáticamente)
                workbook.Save();
            }

            return filePath;
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
