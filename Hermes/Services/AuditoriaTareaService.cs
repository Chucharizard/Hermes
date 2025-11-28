using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class AuditoriaTareaService
    {
        private readonly HermesDbContext _context;

        public AuditoriaTareaService()
        {
            _context = new HermesDbContext();
        }

        /// <summary>
        /// Obtiene todos los registros de auditoría de tareas
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerTodosAsync()
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría filtrados por rango de fechas
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .Where(a => a.FechaHora >= fechaInicio && a.FechaHora <= fechaFin)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de una acción específica (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerPorAccionAsync(string accion)
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .Where(a => a.Accion == accion)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría realizadas por un usuario modificador específico
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerPorModificadorAsync(Guid usuarioModificadorId)
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .Where(a => a.UsuarioIdModificador == usuarioModificadorId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de una tarea específica
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerPorTareaAsync(Guid tareaId)
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .Where(a => a.TareaId == tareaId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría realizadas desde una máquina específica
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerPorMaquinaAsync(string nombreMaquina)
        {
            return await _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .Where(a => a.NombreMaquina.Contains(nombreMaquina))
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría con múltiples filtros
        /// </summary>
        public async Task<List<AuditoriaTarea>> ObtenerConFiltrosAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? accion = null,
            Guid? usuarioModificadorId = null,
            Guid? tareaId = null,
            string? nombreMaquina = null)
        {
            var query = _context.AuditoriasTarea
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Tarea)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaHora <= fechaFin.Value);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);

            if (usuarioModificadorId.HasValue)
                query = query.Where(a => a.UsuarioIdModificador == usuarioModificadorId.Value);

            if (tareaId.HasValue)
                query = query.Where(a => a.TareaId == tareaId.Value);

            if (!string.IsNullOrEmpty(nombreMaquina))
                query = query.Where(a => a.NombreMaquina.Contains(nombreMaquina));

            return await query
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }
    }
}
