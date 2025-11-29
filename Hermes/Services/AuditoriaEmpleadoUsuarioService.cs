using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class AuditoriaEmpleadoUsuarioService
    {
        private readonly HermesDbContext _context;

        public AuditoriaEmpleadoUsuarioService()
        {
            _context = new HermesDbContext();
        }

        /// <summary>
        /// Obtiene todos los registros de auditoría de empleados y usuarios
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerTodosAsync()
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría filtrados por rango de fechas
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.FechaHora >= fechaInicio && a.FechaHora <= fechaFin)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de una tabla específica (EMPLEADO o USUARIO)
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorTablaAsync(string tablaAfectada)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.TablaAfectada == tablaAfectada)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de una acción específica (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorAccionAsync(string accion)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.Accion == accion)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría realizadas por un usuario modificador específico
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorModificadorAsync(Guid usuarioModificadorId)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.UsuarioIdModificador == usuarioModificadorId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de un empleado afectado específico (por CI)
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorEmpleadoAfectadoAsync(int ciEmpleado)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.CiEmpleadoAfectado == ciEmpleado)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de un usuario afectado específico
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerPorUsuarioAfectadoAsync(Guid usuarioId)
        {
            return await _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .Where(a => a.UsuarioIdAfectado == usuarioId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría con múltiples filtros
        /// </summary>
        public async Task<List<AuditoriaEmpleadoUsuario>> ObtenerConFiltrosAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? tablaAfectada = null,
            string? accion = null,
            Guid? usuarioModificadorId = null,
            int? ciEmpleadoAfectado = null,
            Guid? usuarioIdAfectado = null)
        {
            var query = _context.AuditoriasEmpleadoUsuario
                .Include(a => a.UsuarioModificador)
                    .ThenInclude(u => u!.Empleado)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaHora <= fechaFin.Value);

            if (!string.IsNullOrEmpty(tablaAfectada))
                query = query.Where(a => a.TablaAfectada == tablaAfectada);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);

            if (usuarioModificadorId.HasValue)
                query = query.Where(a => a.UsuarioIdModificador == usuarioModificadorId.Value);

            if (ciEmpleadoAfectado.HasValue)
                query = query.Where(a => a.CiEmpleadoAfectado == ciEmpleadoAfectado.Value);

            if (usuarioIdAfectado.HasValue)
                query = query.Where(a => a.UsuarioIdAfectado == usuarioIdAfectado.Value);

            return await query
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }
    }
}
