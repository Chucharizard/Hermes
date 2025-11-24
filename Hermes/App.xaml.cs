using System.Configuration;
using System.Data;
using System.Windows;
using Hermes.Models;
using Hermes.Services;

namespace Hermes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Usuario? UsuarioActual { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cargar el último tema usado al inicio de la aplicación
            var lastTheme = ThemeService.Instance.GetLastUsedTheme();
            ThemeService.Instance.ApplyTheme(lastTheme);
        }
    }
}
