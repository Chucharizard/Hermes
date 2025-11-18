using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void StatsCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string estadoFiltro)
            {
                // Obtener el MainWindow y su ViewModel
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    // Navegar a la bandeja de tareas recibidas
                    // (Por ahora navegamos a la bandeja sin filtro - el filtro lo agregaremos después)
                    mainViewModel.MostrarBandejaTareasRecibidasCommand.Execute(null);
                }
            }
        }
    }
}
