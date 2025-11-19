using System.Windows;

namespace Hermes.Helpers
{
    /// <summary>
    /// Helper para mostrar diálogos y confirmaciones de manera consistente
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Muestra un mensaje de confirmación con estilo personalizado
        /// </summary>
        /// <param name="mensaje">Mensaje a mostrar</param>
        /// <param name="titulo">Título del diálogo</param>
        /// <returns>True si el usuario confirma, False si cancela</returns>
        public static bool MostrarConfirmacion(string mensaje, string titulo = "Confirmación")
        {
            var resultado = MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            );

            return resultado == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Muestra un mensaje de confirmación para eliminación
        /// </summary>
        /// <param name="elemento">Nombre del elemento a eliminar</param>
        /// <returns>True si el usuario confirma, False si cancela</returns>
        public static bool ConfirmarEliminacion(string elemento)
        {
            var mensaje = $"¿Está seguro que desea eliminar '{elemento}'?\n\n" +
                         "Esta acción no se puede deshacer.";

            return MostrarConfirmacion(mensaje, "⚠️ Confirmar Eliminación");
        }

        /// <summary>
        /// Muestra un mensaje de éxito
        /// </summary>
        /// <param name="mensaje">Mensaje de éxito</param>
        /// <param name="titulo">Título del diálogo</param>
        public static void MostrarExito(string mensaje, string titulo = "✓ Éxito")
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        /// <param name="mensaje">Mensaje de error</param>
        /// <param name="titulo">Título del diálogo</param>
        public static void MostrarError(string mensaje, string titulo = "❌ Error")
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /// <summary>
        /// Muestra un mensaje de advertencia
        /// </summary>
        /// <param name="mensaje">Mensaje de advertencia</param>
        /// <param name="titulo">Título del diálogo</param>
        public static void MostrarAdvertencia(string mensaje, string titulo = "⚠️ Advertencia")
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        /// <summary>
        /// Muestra un mensaje informativo
        /// </summary>
        /// <param name="mensaje">Mensaje informativo</param>
        /// <param name="titulo">Título del diálogo</param>
        public static void MostrarInformacion(string mensaje, string titulo = "ℹ️ Información")
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
