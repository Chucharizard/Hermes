using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace Hermes.Helpers
{
    /// <summary>
    /// Clase centralizada para el hash y verificación de contraseñas
    /// Utiliza BCrypt (estándar de la industria) con salt automático
    /// Mantiene compatibilidad con passwords antiguos en SHA256
    /// </summary>
    public static class PasswordHasher
    {
        // Factor de trabajo de BCrypt (12 es el recomendado para 2024)
        // Más alto = más seguro pero más lento
        private const int BCRYPT_WORK_FACTOR = 12;

        /// <summary>
        /// Hashea una contraseña usando BCrypt con salt automático
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash BCrypt (incluye salt automático)</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // BCrypt genera automáticamente un salt único
            return BCrypt.Net.BCrypt.HashPassword(password, BCRYPT_WORK_FACTOR);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash
        /// Soporta tanto BCrypt (nuevo) como SHA256 (legacy)
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hash">Hash almacenado en BD</param>
        /// <returns>True si la contraseña es correcta</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            // Detectar si es BCrypt o SHA256 legacy
            if (IsBCryptHash(hash))
            {
                // Nuevo sistema: BCrypt
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            else
            {
                // Sistema legacy: SHA256 (para migración gradual)
                return VerifyLegacySHA256(password, hash);
            }
        }

        /// <summary>
        /// Determina si un hash debe ser actualizado
        /// Retorna true si es SHA256 legacy o si BCrypt necesita rehash
        /// </summary>
        /// <param name="hash">Hash almacenado</param>
        /// <returns>True si debe actualizarse a BCrypt moderno</returns>
        public static bool NeedsRehash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return true;

            // Si es SHA256, definitivamente necesita rehash
            if (!IsBCryptHash(hash))
                return true;

            // Si es BCrypt pero con factor de trabajo antiguo, también necesita rehash
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, BCRYPT_WORK_FACTOR);
        }

        /// <summary>
        /// Detecta si un hash es BCrypt (empieza con $2a$, $2b$, $2y$)
        /// </summary>
        private static bool IsBCryptHash(string hash)
        {
            return hash.StartsWith("$2a$") ||
                   hash.StartsWith("$2b$") ||
                   hash.StartsWith("$2y$");
        }

        /// <summary>
        /// Verifica contraseña usando SHA256 (sistema legacy - solo para compatibilidad)
        /// NO USAR PARA NUEVAS CONTRASEÑAS
        /// </summary>
        private static bool VerifyLegacySHA256(string password, string hash)
        {
            string passwordHash = HashPasswordSHA256Legacy(password);

            // Comparación insensible a mayúsculas para compatibilidad
            return string.Equals(
                passwordHash.Trim(),
                hash.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// Hash SHA256 legacy - SOLO para verificar passwords antiguos
        /// NO USAR para crear nuevos hashes
        /// </summary>
        [Obsolete("Solo usar para verificar passwords legacy. Usar HashPassword() para nuevos passwords.")]
        private static string HashPasswordSHA256Legacy(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Genera una contraseña aleatoria segura
        /// Útil para reseteo de contraseñas
        /// </summary>
        /// <param name="length">Longitud de la contraseña (mínimo 8)</param>
        /// <returns>Contraseña aleatoria</returns>
        public static string GenerateRandomPassword(int length = 12)
        {
            if (length < 8)
                throw new ArgumentException("La longitud mínima es 8 caracteres", nameof(length));

            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                char[] chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = validChars[randomBytes[i] % validChars.Length];
                }

                return new string(chars);
            }
        }
    }
}
