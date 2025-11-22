using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hermes.ViewModels;
using Hermes.Models;

namespace Hermes.Views
{
    public partial class BandejaTareasRecibidasView : UserControl
    {
        public BandejaTareasRecibidasView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Manejador para doble click en un item de tarea
        /// Establece la tarea seleccionada para mostrar el detalle inline
        /// </summary>
        private void TareaItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Tarea tarea)
            {
                // Establecer la tarea seleccionada en el ViewModel
                // Esto hará que se muestre el DetalleTareaView inline
                if (DataContext is BandejaTareasRecibidasViewModel viewModel)
                {
                    viewModel.TareaSeleccionada = tarea;
                }
            }
        }
    }
}
