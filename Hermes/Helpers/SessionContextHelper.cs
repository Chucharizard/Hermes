using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Hermes.Helpers
{
    /// <summary>
    /// Helper para establecer el contexto de sesión en SQL Server
    /// Esto permite que los triggers de auditoría capturen quién hizo la modificación
    /// </summary>
    public static class SessionContextHelper
    {
        /// <summary>
        /// Establece el contexto de sesión para auditoría (UsuarioId y CiEmpleado)
        /// Debe llamarse ANTES de cualquier operación que dispare triggers de auditoría
        /// </summary>
        public static async Task EstablecerContextoAsync(SqlConnection conexion, Guid usuarioId, int ciEmpleado)
        {
            try
            {
                // Convertir GUID a string para SESSION_CONTEXT
                string usuarioIdStr = usuarioId.ToString();
                string ciEmpleadoStr = ciEmpleado.ToString();

                // Establecer UsuarioId en SESSION_CONTEXT
                using (var cmdUsuario = new SqlCommand("EXEC sp_set_session_context @key = N'UsuarioId', @value = @usuarioId", conexion))
                {
                    cmdUsuario.Parameters.AddWithValue("@usuarioId", usuarioIdStr);
                    await cmdUsuario.ExecuteNonQueryAsync();
                }

                // Establecer CiEmpleado en SESSION_CONTEXT
                using (var cmdCi = new SqlCommand("EXEC sp_set_session_context @key = N'CiEmpleado', @value = @ciEmpleado", conexion))
                {
                    cmdCi.Parameters.AddWithValue("@ciEmpleado", ciEmpleadoStr);
                    await cmdCi.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al establecer SESSION_CONTEXT: {ex.Message}");
                // No lanzar excepción para no bloquear la operación principal
            }
        }

        /// <summary>
        /// Establece el contexto de sesión usando el usuario actual de la aplicación
        /// </summary>
        public static async Task EstablecerContextoUsuarioActualAsync(SqlConnection conexion)
        {
            if (App.UsuarioActual != null)
            {
                await EstablecerContextoAsync(
                    conexion,
                    App.UsuarioActual.IdUsuario,
                    App.UsuarioActual.EmpleadoCi
                );
            }
        }
    }
}
