using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class TareaService
    {
        private readonly HermesDbContext _context;

        public TareaService()
        {
            _context = new HermesDbContext();
        }

        // READ ALL
        public async Task<List<Tarea>> ObtenerTodasAsync()
        {
            return await _context.Tareas
                .Include(t => t.UsuarioEmisor)
                    .ThenInclude(u => u!.Empleado)
                .Include(t => t.UsuarioReceptor)
                    .ThenInclude(u => u!.Empleado)
                .OrderByDescending(t => t.FechaInicioTarea)
                .ToListAsync();
        }

        // READ BY ID
        public async Task<Tarea?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.Tareas
                .Include(t => t.UsuarioEmisor)
                    .ThenInclude(u => u!.Empleado)
                .Include(t => t.UsuarioReceptor)
                    .ThenInclude(u => u!.Empleado)
                .FirstOrDefaultAsync(t => t.IdTarea == id);
        }

        // READ BY USUARIO EMISOR
        public async Task<List<Tarea>> ObtenerPorEmisorAsync(Guid usuarioEmisorId)
        {
            return await _context.Tareas
                .Include(t => t.UsuarioEmisor)
                    .ThenInclude(u => u!.Empleado)
                .Include(t => t.UsuarioReceptor)
                    .ThenInclude(u => u!.Empleado)
                .Where(t => t.UsuarioEmisorId == usuarioEmisorId)
                .OrderByDescending(t => t.FechaInicioTarea)
                .ToListAsync();
        }

        // READ BY USUARIO RECEPTOR
        public async Task<List<Tarea>> ObtenerPorReceptorAsync(Guid usuarioReceptorId)
        {
            return await _context.Tareas
                .Include(t => t.UsuarioEmisor)
                    .ThenInclude(u => u!.Empleado)
                .Include(t => t.UsuarioReceptor)
                    .ThenInclude(u => u!.Empleado)
                .Where(t => t.UsuarioReceptorId == usuarioReceptorId)
                .OrderByDescending(t => t.FechaInicioTarea)
                .ToListAsync();
        }

        // CREATE
        public async Task<bool> CrearAsync(Tarea tarea)
        {
            try
            {
                _context.Tareas.Add(tarea);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // UPDATE
        public async Task<bool> ActualizarAsync(Tarea tarea)
        {
            try
            {
                var tareaExistente = await ObtenerPorIdAsync(tarea.IdTarea);

                if (tareaExistente == null)
                    return false;

                // Actualizar propiedades
                tareaExistente.UsuarioEmisorId = tarea.UsuarioEmisorId;
                tareaExistente.UsuarioReceptorId = tarea.UsuarioReceptorId;
                tareaExistente.TituloTarea = tarea.TituloTarea;
                tareaExistente.DescripcionTarea = tarea.DescripcionTarea;
                tareaExistente.EstadoTarea = tarea.EstadoTarea;
                tareaExistente.PrioridadTarea = tarea.PrioridadTarea;
                tareaExistente.FechaInicioTarea = tarea.FechaInicioTarea;
                tareaExistente.FechaLimiteTarea = tarea.FechaLimiteTarea;
                tareaExistente.FechaCompletadaTarea = tarea.FechaCompletadaTarea;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // DELETE
        public async Task<bool> EliminarAsync(Guid id)
        {
            try
            {
                var tarea = await ObtenerPorIdAsync(id);
                if (tarea != null)
                {
                    _context.Tareas.Remove(tarea);
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

        /// <summary>
        /// Actualiza automáticamente el estado de tareas que han pasado su fecha límite
        /// Similar al comportamiento de Microsoft Teams
        /// </summary>
        public async Task<int> ActualizarTareasVencidasAsync()
        {
            try
            {
                var ahora = DateTime.Now;

                // Buscar tareas que:
                // 1. Tienen fecha límite
                // 2. La fecha límite ya pasó
                // 3. NO están completadas ni archivadas
                // 4. NO están ya marcadas como vencidas
                var tareasVencidas = await _context.Tareas
                    .Where(t =>
                        t.FechaLimiteTarea.HasValue &&
                        t.FechaLimiteTarea.Value < ahora &&
                        t.EstadoTarea != "Completado" &&
                        t.EstadoTarea != "Archivado" &&
                        t.EstadoTarea != "Vencido")
                    .ToListAsync();

                // Marcar como vencidas
                foreach (var tarea in tareasVencidas)
                {
                    tarea.EstadoTarea = "Vencido";
                }

                if (tareasVencidas.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return tareasVencidas.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Verifica si una tarea específica está vencida y la actualiza si es necesario
        /// </summary>
        public async Task<bool> VerificarYActualizarVencimientoAsync(Guid tareaId)
        {
            try
            {
                var tarea = await ObtenerPorIdAsync(tareaId);

                if (tarea == null) return false;

                // Si ya está completada o archivada, no hacer nada
                if (tarea.EstadoTarea == "Completado" || tarea.EstadoTarea == "Archivado")
                    return false;

                // Si tiene fecha límite y ya pasó
                if (tarea.FechaLimiteTarea.HasValue &&
                    tarea.FechaLimiteTarea.Value < DateTime.Now &&
                    tarea.EstadoTarea != "Vencido")
                {
                    tarea.EstadoTarea = "Vencido";
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

        /// <summary>
        /// Permite completar una tarea vencida (completada con retraso)
        /// Similar a Teams que permite completar tareas vencidas
        /// </summary>
        public async Task<bool> CompletarTareaVencidaAsync(Guid tareaId, string? comentarioRetraso = null)
        {
            try
            {
                var tarea = await ObtenerPorIdAsync(tareaId);

                if (tarea == null) return false;

                tarea.EstadoTarea = "Completado";
                tarea.FechaCompletadaTarea = DateTime.Now;

                await _context.SaveChangesAsync();

                // Nota: Si se desea, aquí se puede agregar un comentario automático
                // indicando que la tarea fue completada con retraso

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
