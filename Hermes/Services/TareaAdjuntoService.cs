using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;
using System.IO;

namespace Hermes.Services
{
    public class TareaAdjuntoService
    {
        private readonly HermesDbContext _context;

        public TareaAdjuntoService()
        {
            _context = new HermesDbContext();
        }

        // READ ALL ATTACHMENTS FOR A TASK
        public async Task<List<TareaAdjunto>> ObtenerAdjuntosPorTareaAsync(Guid idTarea)
        {
            try
            {
                var adjuntos = await _context.TareasAdjuntos
                    .Where(ta => ta.TareaId == idTarea)
                    .OrderByDescending(ta => ta.FechaSubidaTareaAdjunto)
                    .ToListAsync();

                // Cargar manualmente los usuarios para evitar problemas con FK nullable
                foreach (var adjunto in adjuntos)
                {
                    if (adjunto.IdUsuarioSubioTareaAdjunto.HasValue)
                    {
                        var usuario = await _context.Usuarios
                            .Include(u => u.Empleado)
                            .FirstOrDefaultAsync(u => u.IdUsuario == adjunto.IdUsuarioSubioTareaAdjunto.Value);

                        adjunto.UsuarioSubioNavigation = usuario;
                    }
                }

                return adjuntos;
            }
            catch (Exception)
            {
                // Si falla, retornar lista vacía en vez de lanzar excepción
                return new List<TareaAdjunto>();
            }
        }

        // READ BY ID
        public async Task<TareaAdjunto?> ObtenerPorIdAsync(Guid id)
        {
            try
            {
                var adjunto = await _context.TareasAdjuntos
                    .Include(ta => ta.Tarea)
                    .FirstOrDefaultAsync(ta => ta.IdTareaAdjunto == id);

                if (adjunto != null && adjunto.IdUsuarioSubioTareaAdjunto.HasValue)
                {
                    adjunto.UsuarioSubioNavigation = await _context.Usuarios
                        .Include(u => u.Empleado)
                        .FirstOrDefaultAsync(u => u.IdUsuario == adjunto.IdUsuarioSubioTareaAdjunto.Value);
                }

                return adjunto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // UPLOAD FILE
        public async Task<bool> SubirAdjuntoAsync(Guid idTarea, Guid idUsuarioSubio, string rutaArchivoOrigen)
        {
            try
            {
                if (!File.Exists(rutaArchivoOrigen))
                    return false;

                var fileInfo = new FileInfo(rutaArchivoOrigen);

                // Leer archivo como bytes
                byte[] archivoBytes = await File.ReadAllBytesAsync(rutaArchivoOrigen);

                // Crear registro en base de datos con el archivo como VARBINARY
                var adjunto = new TareaAdjunto
                {
                    IdTareaAdjunto = Guid.NewGuid(),
                    TareaId = idTarea,
                    NombreArchivoTareaAdjunto = fileInfo.Name,
                    TipoArchivoTareaAdjunto = fileInfo.Extension,
                    ArchivoTareaAdjunto = archivoBytes,
                    IdUsuarioSubioTareaAdjunto = idUsuarioSubio,
                    FechaSubidaTareaAdjunto = DateTime.Now
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

            if (adjunto?.ArchivoTareaAdjunto != null)
            {
                // Crear archivo temporal con el contenido del archivo de la BD
                var tempPath = Path.Combine(Path.GetTempPath(), adjunto.NombreArchivoTareaAdjunto);

                // Si ya existe el archivo temporal, eliminarlo primero
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                await File.WriteAllBytesAsync(tempPath, adjunto.ArchivoTareaAdjunto);
                return tempPath;
            }

            return null;
        }

        // COUNT ATTACHMENTS FOR A TASK
        public async Task<int> ContarAdjuntosPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasAdjuntos
                .Where(ta => ta.TareaId == idTarea)
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
                    // Eliminar solo el registro de BD (el archivo está dentro de la BD)
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
            var adjuntos = await _context.TareasAdjuntos
                .Where(ta => ta.TareaId == idTarea)
                .ToListAsync();

            return adjuntos.Sum(ta => ta.ArchivoTareaAdjunto?.Length ?? 0);
        }
    }
}
