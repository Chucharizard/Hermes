using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class AuditoriaSesionService
    {
        private readonly HermesDbContext _context;

        public AuditoriaSesionService()
        {
            _context = new HermesDbContext();
        }

        /// <summary>
        /// Registra un evento de inicio de sesión (LOGIN)
        /// </summary>
        public async Task<bool> RegistrarLoginAsync(Guid usuarioId, int ciEmpleado, string nombreMaquina)
        {
            try
            {
                var auditoria = new AuditoriaSesion
                {
                    IdAuditoriaSesion = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    CiEmpleado = ciEmpleado,
                    FechaHora = DateTime.Now,
                    NombreMaquina = nombreMaquina,
                    TipoEvento = "LOGIN"
                };

                _context.AuditoriasSesion.Add(auditoria);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registra un evento de cierre de sesión (LOGOUT)
        /// </summary>
        public async Task<bool> RegistrarLogoutAsync(Guid usuarioId, int ciEmpleado, string nombreMaquina)
        {
            try
            {
                var auditoria = new AuditoriaSesion
                {
                    IdAuditoriaSesion = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    CiEmpleado = ciEmpleado,
                    FechaHora = DateTime.Now,
                    NombreMaquina = nombreMaquina,
                    TipoEvento = "LOGOUT"
                };

                _context.AuditoriasSesion.Add(auditoria);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene todos los registros de auditoría de sesiones
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerTodosAsync()
        {
            return await _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría filtrados por rango de fechas
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .Where(a => a.FechaHora >= fechaInicio && a.FechaHora <= fechaFin)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de un usuario específico
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            return await _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría de un empleado específico (por CI)
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerPorEmpleadoAsync(int ciEmpleado)
        {
            return await _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .Where(a => a.CiEmpleado == ciEmpleado)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría filtrados por tipo de evento (LOGIN/LOGOUT)
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerPorTipoEventoAsync(string tipoEvento)
        {
            return await _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .Where(a => a.TipoEvento == tipoEvento)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene registros de auditoría con múltiples filtros
        /// </summary>
        public async Task<List<AuditoriaSesion>> ObtenerConFiltrosAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            Guid? usuarioId = null,
            int? ciEmpleado = null,
            string? tipoEvento = null)
        {
            var query = _context.AuditoriasSesion
                .Include(a => a.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(a => a.Empleado)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaHora <= fechaFin.Value);

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (ciEmpleado.HasValue)
                query = query.Where(a => a.CiEmpleado == ciEmpleado.Value);

            if (!string.IsNullOrEmpty(tipoEvento))
                query = query.Where(a => a.TipoEvento == tipoEvento);

            return await query
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }
    }
}
