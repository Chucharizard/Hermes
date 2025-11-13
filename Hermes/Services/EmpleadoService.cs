using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class EmpleadoService
    {
        private readonly HermesDbContext _context;

        public EmpleadoService()
        {
            _context = new HermesDbContext();
        }

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            return await _context.Empleados
                .OrderBy(e => e.ApellidosEmpleado)
                .ToListAsync();
        }

        public async Task<Empleado?> ObtenerPorCiAsync(int ci)
        {
            return await _context.Empleados
                .FirstOrDefaultAsync(e => e.CiEmpleado == ci);
        }

        public async Task<bool> CrearAsync(Empleado empleado)
        {
            try
            {
                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(Empleado empleado)
        {
            try
            {
                _context.Empleados.Update(empleado);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int ci)
        {
            try
            {
                var empleado = await ObtenerPorCiAsync(ci);
                if (empleado != null)
                {
                    empleado.EsActivoEmpleado = false;
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
