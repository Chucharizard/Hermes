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
        /// El clic simple (manteniendo pulsado) permite arrastrar la tarea
        /// </summary>
        private void TareaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Solo actuar si es doble clic (ClickCount == 2)
            if (e.ClickCount == 2 && sender is Border border && border.Tag is Tarea tarea)
            {
                // Establecer la tarea seleccionada en el ViewModel
                // Esto hará que se muestre el DetalleTareaView inline
                if (DataContext is BandejaTareasRecibidasViewModel viewModel)
                {
                    viewModel.TareaSeleccionada = tarea;
                }

                // Marcar el evento como manejado para que no interfiera con el drag & drop
                e.Handled = true;
            }
            // Si ClickCount == 1, no hacemos nada y dejamos que el drag & drop funcione
        }
    }
}
