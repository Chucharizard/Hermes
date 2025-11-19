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
        /// Manejador para el click en un item de tarea
        /// Muestra el panel lateral integrado con el detalle (estilo Teams)
        /// </summary>
        private void TareaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Tarea tarea)
            {
                // Seleccionar la tarea - esto muestra el panel lateral automáticamente
                if (DataContext is BandejaTareasRecibidasViewModel viewModel)
                {
                    viewModel.TareaSeleccionada = tarea;
                }
            }
        }
    }
}
