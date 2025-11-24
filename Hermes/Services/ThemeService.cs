using System;
using System.Linq;
using System.Windows;
using Hermes.Data;

namespace Hermes.Services
{
    /// <summary>
    /// Servicio singleton para gestionar cambios dinámicos de tema en la aplicación
    /// </summary>
    public class ThemeService
    {
        private static ThemeService? _instance;
        private static readonly object _lock = new object();

        // Singleton pattern
        public static ThemeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ThemeService();
                        }
                    }
                }
                return _instance;
            }
        }

        private ThemeService()
        {
            // Constructor privado para patrón singleton
        }

        /// <summary>
        /// Cambia el tema activo de la aplicación
        /// </summary>
        /// <param name="themeName">Nombre del tema (Emerald, Purple, Classic)</param>
        /// <returns>True si el cambio fue exitoso, False si hubo error</returns>
        public bool ApplyTheme(string themeName)
        {
            try
            {
                // Construir URI del tema
                var themeUri = new Uri($"pack://application:,,,/Resources/Themes/Theme.{themeName}.xaml");

                // Crear nuevo ResourceDictionary con el tema
                var newTheme = new ResourceDictionary { Source = themeUri };

                // Encontrar el índice del tema actual en MergedDictionaries
                var stylesDict = Application.Current.Resources.MergedDictionaries[0];

                if (stylesDict.MergedDictionaries.Count > 0)
                {
                    // Reemplazar el tema actual (siempre está en índice 0)
                    stylesDict.MergedDictionaries[0] = newTheme;
                }
                else
                {
                    // Si no hay tema cargado, agregarlo
                    stylesDict.MergedDictionaries.Add(newTheme);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar tema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre del tema actualmente aplicado
        /// </summary>
        /// <returns>Nombre del tema actual (Emerald, Purple, etc.)</returns>
        public string GetCurrentTheme()
        {
            try
            {
                var stylesDict = Application.Current.Resources.MergedDictionaries[0];

                if (stylesDict.MergedDictionaries.Count > 0)
                {
                    var themeUri = stylesDict.MergedDictionaries[0].Source?.ToString();

                    if (!string.IsNullOrEmpty(themeUri))
                    {
                        // Extraer nombre del tema de la URI
                        // Ejemplo: "pack://application:,,,/Resources/Themes/Theme.Emerald.xaml"
                        var themeName = themeUri.Split('/').Last().Replace("Theme.", "").Replace(".xaml", "");
                        return themeName;
                    }
                }
            }
            catch
            {
                // Si hay error, asumir tema Emerald por defecto
            }

            return "Emerald"; // Tema por defecto
        }

        /// <summary>
        /// Obtiene lista de temas disponibles
        /// </summary>
        public string[] GetAvailableThemes()
        {
            return new[] { "Emerald", "Purple" };
        }

        /// <summary>
        /// Guarda la preferencia de tema del usuario en la base de datos
        /// </summary>
        /// <param name="themeName">Nombre del tema a guardar</param>
        /// <returns>True si se guardó correctamente, False si hubo error</returns>
        public bool SaveUserThemePreference(string themeName)
        {
            try
            {
                // Verificar que hay un usuario en sesión
                if (App.UsuarioActual == null)
                    return false;

                using (var context = new HermesDbContext())
                {
                    // Buscar el usuario actual en la base de datos
                    var usuario = context.Usuarios.FirstOrDefault(u => u.IdUsuario == App.UsuarioActual.IdUsuario);

                    if (usuario != null)
                    {
                        // Actualizar el tema preferido
                        usuario.TemaPreferido = themeName;
                        context.SaveChanges();

                        // Actualizar también en la sesión actual
                        App.UsuarioActual.TemaPreferido = themeName;

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar preferencia de tema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
