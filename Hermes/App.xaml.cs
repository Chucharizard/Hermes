using System.Configuration;
using System.Data;
using System.Windows;
using Hermes.Models;
using Hermes.Helpers;

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

            // Inicializar el tema
            ThemeManager.Initialize();
        }
    }
}
