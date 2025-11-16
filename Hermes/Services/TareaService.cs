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
    }
}
