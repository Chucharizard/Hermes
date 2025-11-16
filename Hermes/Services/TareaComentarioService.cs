using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class TareaComentarioService
    {
        private readonly HermesDbContext _context;

        public TareaComentarioService()
        {
            _context = new HermesDbContext();
        }

        // READ ALL COMMENTS FOR A TASK
        public async Task<List<TareaComentario>> ObtenerComentariosPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasComentarios
                .Include(tc => tc.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Where(tc => tc.TareaIdComentario == idTarea)
                .OrderBy(tc => tc.FechaTareaComentario)
                .ToListAsync();
        }

        // READ BY ID
        public async Task<TareaComentario?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.TareasComentarios
                .Include(tc => tc.Usuario)
                    .ThenInclude(u => u!.Empleado)
                .Include(tc => tc.Tarea)
                .FirstOrDefaultAsync(tc => tc.IdTareaComentario == id);
        }

        // CREATE
        public async Task<bool> AgregarComentarioAsync(TareaComentario comentario)
        {
            try
            {
                comentario.IdTareaComentario = Guid.NewGuid();
                comentario.FechaTareaComentario = DateTime.Now;

                _context.TareasComentarios.Add(comentario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // COUNT COMMENTS FOR A TASK
        public async Task<int> ContarComentariosPorTareaAsync(Guid idTarea)
        {
            return await _context.TareasComentarios
                .Where(tc => tc.TareaIdComentario == idTarea)
                .CountAsync();
        }

        // DELETE
        public async Task<bool> EliminarAsync(Guid id)
        {
            try
            {
                var comentario = await ObtenerPorIdAsync(id);
                if (comentario != null)
                {
                    _context.TareasComentarios.Remove(comentario);
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
