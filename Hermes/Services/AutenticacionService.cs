using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;
using Hermes.Helpers;

namespace Hermes.Services
{
    public class AutenticacionService
    {
        private readonly HermesDbContext _context;

        public AutenticacionService()
        {
            _context = new HermesDbContext();
        }

        /// <summary>
        /// Valida las credenciales de un usuario y actualiza automáticamente
        /// las contraseñas legacy (SHA256) a BCrypt en el primer login exitoso
        /// </summary>
        public async Task<(bool Success, Usuario? Usuario, string Mensaje)> ValidarCredencialesAsync(int ciEmpleado, string password)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Empleado)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.EmpleadoCi == ciEmpleado && u.EsActivoUsuario);

                if (usuario == null)
                {
                    return (false, null, "Usuario no encontrado o inactivo");
                }

                // Verificar contraseña usando la clase centralizada
                // Soporta tanto BCrypt (nuevo) como SHA256 (legacy)
                bool passwordValida = PasswordHasher.VerifyPassword(password, usuario.PasswordUsuario);

                if (!passwordValida)
                {
                    return (false, null, "Contraseña incorrecta");
                }

                // MIGRACIÓN AUTOMÁTICA: Si el password es correcto pero está en SHA256 legacy,
                // actualizarlo a BCrypt en este momento
                if (PasswordHasher.NeedsRehash(usuario.PasswordUsuario))
                {
                    await MigrarPasswordABCryptAsync(usuario, password);
                }

                return (true, usuario, "Autenticación exitosa");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error de autenticación: {ex.Message}");
            }
        }

        /// <summary>
        /// Migra automáticamente un password de SHA256 a BCrypt
        /// Se ejecuta transparentemente durante el login
        /// </summary>
        private async Task MigrarPasswordABCryptAsync(Usuario usuario, string passwordTextoPlano)
        {
            try
            {
                // Generar nuevo hash BCrypt
                string nuevoHashBCrypt = PasswordHasher.HashPassword(passwordTextoPlano);

                // Actualizar en BD
                usuario.PasswordUsuario = nuevoHashBCrypt;
                await _context.SaveChangesAsync();

                // Log silencioso de la migración (opcional)
                System.Diagnostics.Debug.WriteLine(
                    $"Password migrado a BCrypt para usuario: {usuario.NombreUsuario}"
                );
            }
            catch (Exception ex)
            {
                // Si falla la migración, no afecta el login
                // El usuario puede seguir usando su password legacy
                System.Diagnostics.Debug.WriteLine(
                    $"Error al migrar password: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Crea un hash de contraseña usando BCrypt
        /// Usar este método al crear o actualizar contraseñas
        /// </summary>
        public string HashPassword(string password)
        {
            return PasswordHasher.HashPassword(password);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            return PasswordHasher.VerifyPassword(password, hash);
        }
    }
}
