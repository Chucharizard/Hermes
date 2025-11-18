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
        /// Manejador para el click en un card de tarea
        /// Selecciona la tarea y puede ejecutar Ver Detalle
        /// </summary>
        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Tarea tarea)
            {
                // Obtener el ViewModel y seleccionar la tarea
                if (DataContext is BandejaTareasEnviadasViewModel viewModel)
                {
                    viewModel.TareaSeleccionada = tarea;
                }
            }
        }
    }
}
