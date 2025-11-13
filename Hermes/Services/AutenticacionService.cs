using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services
{
    public class AutenticacionService
    {
        private readonly HermesDbContext _context;

        public AutenticacionService()
        {
            _context = new HermesDbContext();
        }

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

                // Hashear la contraseña ingresada
                string passwordHash = HashPassword(password);

                // IMPORTANTE: Limpiar espacios y convertir a minúsculas para comparación
                string hashBD = usuario.PasswordUsuario?.Trim().ToLower() ?? string.Empty;
                string hashGenerado = passwordHash?.Trim().ToLower() ?? string.Empty;

                // Comparar hashes
                if (hashBD != hashGenerado)
                {
                    return (false, null, "Contraseña incorrecta");
                }

                return (true, usuario, "Autenticación exitosa");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error de autenticación: {ex.Message}");
            }
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // minúsculas (hexadecimal)
                }
                return builder.ToString();
            }
        }

        // Método para verificar hash (útil para debugging)
        public void VerificarHash(string password, string hashEsperado)
        {
            string hashGenerado = HashPassword(password);

            MessageBox.Show(
                $"Password: {password}\n\n" +
                $"Hash generado:\n{hashGenerado}\n\n" +
                $"Hash esperado:\n{hashEsperado}\n\n" +
                $"¿Coinciden?: {hashGenerado.ToLower() == hashEsperado.ToLower()}",
                "Verificación de Hash",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
