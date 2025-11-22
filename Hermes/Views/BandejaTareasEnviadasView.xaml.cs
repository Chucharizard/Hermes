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
        /// Abre una ventana modal con los detalles de la tarea
        /// </summary>
        private void TareaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Tarea tarea)
            {
                // Crear y mostrar ventana modal de detalle
                var detalleWindow = new DetalleTareaWindow();
                var detalleViewModel = new DetalleTareaViewModel(tarea);
                detalleWindow.DataContext = detalleViewModel;

                // Mostrar como diálogo modal
                detalleWindow.ShowDialog();

                // Refrescar la lista de tareas después de cerrar la ventana de detalle
                if (DataContext is BandejaTareasEnviadasViewModel viewModel)
                {
                    viewModel.RefrescarCommand.Execute(null);
                }
            }
        }
    }
}
