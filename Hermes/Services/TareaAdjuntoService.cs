using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;
using System.IO;

namespace Hermes.Services
{
    public class TareaAdjuntoService
    {
        private readonly HermesDbContext _context;
        private readonly string _adjuntosDirectory;

        public TareaAdjuntoService()
        {
            _context = new HermesDbContext();

            // Directorio para guardar archivos adjuntos
            _adjuntosDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Adjuntos");

            // Crear directorio si no existe
            if (!Directory.Exists(_adjuntosDirectory))
            {
                Directory.CreateDirectory(_adjuntosDirectory);
            }
        }

        // READ ALL ATTACHMENTS FOR A TASK
        public async Task<List<TareaAdjunto>> ObtenerAdjuntosPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasAdjuntos
                .Include(ta => ta.UsuarioSubio)
                    .ThenInclude(u => u!.Empleado)
                .Where(ta => ta.IdTarea == idTarea)
                .OrderByDescending(ta => ta.FechaSubida)
                .ToListAsync();
        }

        // READ BY ID
        public async Task<TareaAdjunto?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.TareasAdjuntos
                .Include(ta => ta.UsuarioSubio)
                    .ThenInclude(u => u!.Empleado)
                .Include(ta => ta.Tarea)
                .FirstOrDefaultAsync(ta => ta.IdAdjunto == id);
        }

        // UPLOAD FILE
        public async Task<bool> SubirAdjuntoAsync(Guid idTarea, Guid idUsuarioSubio, string rutaArchivoOrigen)
        {
            try
            {
                if (!File.Exists(rutaArchivoOrigen))
                    return false;

                var fileInfo = new FileInfo(rutaArchivoOrigen);

                // Generar nombre único para el archivo
                var nombreUnico = $"{Guid.NewGuid()}_{fileInfo.Name}";
                var rutaDestino = Path.Combine(_adjuntosDirectory, nombreUnico);

                // Copiar archivo al directorio de adjuntos
                File.Copy(rutaArchivoOrigen, rutaDestino);

                // Crear registro en base de datos
                var adjunto = new TareaAdjunto
                {
                    IdAdjunto = Guid.NewGuid(),
                    IdTarea = idTarea,
                    NombreArchivo = fileInfo.Name,
                    RutaArchivo = rutaDestino,
                    TipoArchivo = fileInfo.Extension,
                    TamañoArchivo = fileInfo.Length,
                    IdUsuarioSubio = idUsuarioSubio,
                    FechaSubida = DateTime.Now
                };

                _context.TareasAdjuntos.Add(adjunto);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // DOWNLOAD FILE (returns file path)
        public async Task<string?> ObtenerRutaArchivoAsync(Guid idAdjunto)
        {
            var adjunto = await ObtenerPorIdAsync(idAdjunto);

            if (adjunto != null && File.Exists(adjunto.RutaArchivo))
            {
                return adjunto.RutaArchivo;
            }

            return null;
        }

        // COUNT ATTACHMENTS FOR A TASK
        public async Task<int> ContarAdjuntosPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasAdjuntos
                .Where(ta => ta.IdTarea == idTarea)
                .CountAsync();
        }

        // DELETE
        public async Task<bool> EliminarAsync(Guid id)
        {
            try
            {
                var adjunto = await ObtenerPorIdAsync(id);
                if (adjunto != null)
                {
                    // Eliminar archivo físico
                    if (File.Exists(adjunto.RutaArchivo))
                    {
                        File.Delete(adjunto.RutaArchivo);
                    }

                    // Eliminar registro de BD
                    _context.TareasAdjuntos.Remove(adjunto);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // GET TOTAL SIZE OF ATTACHMENTS FOR A TASK
        public async Task<long> ObtenerTamañoTotalPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasAdjuntos
                .Where(ta => ta.IdTarea == idTarea)
                .SumAsync(ta => ta.TamañoArchivo);
        }
    }
}
