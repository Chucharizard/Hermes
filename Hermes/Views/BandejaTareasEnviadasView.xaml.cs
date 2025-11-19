using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hermes.ViewModels;
using Hermes.Models;

namespace Hermes.Views
{
    public partial class BandejaTareasEnviadasView : UserControl
    {
        public BandejaTareasEnviadasView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Manejador para el click en un item de tarea
        /// Abre la ventana de detalle en modo modal (estilo Teams)
        /// </summary>
        private void TareaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Tarea tarea)
            {
                // Abrir ventana modal con el detalle de la tarea
                var ventanaDetalle = new DetalleTareaWindow(tarea);
                ventanaDetalle.Owner = Application.Current.MainWindow;

                if (ventanaDetalle.ShowDialog() == true)
                {
                    // Refrescar la lista si hubo cambios
                    if (DataContext is BandejaTareasEnviadasViewModel viewModel)
                    {
                        viewModel.RefrescarCommand.Execute(null);
                    }
                }
            }
        }
    }
}
