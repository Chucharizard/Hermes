using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class RolService
    {
        private readonly HermesDbContext _context;

        public RolService()
        {
            _context = new HermesDbContext();
        }

        public async Task<List<Rol>> ObtenerActivosAsync()
        {
            return await _context.Roles
                .Where(r => r.EsActivoRol)
                .OrderBy(r => r.NombreRol)
                .ToListAsync();
        }

        public async Task<Rol?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.IdRol == id);
        }

        public async Task<List<Rol>> ObtenerTodosAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.NombreRol)
                .ToListAsync();
        }
    }
}
