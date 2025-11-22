using System.Windows.Controls;
using System.Windows.Input;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class GestionTareasView : UserControl
    {
        public GestionTareasView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Manejador para el doble click en un item de tarea
        /// Abre el panel de edición en la columna derecha
        /// </summary>
        private void TareaItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Solo responder a doble click con botón izquierdo
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                if (sender is Border border && border.Tag is Tarea tarea)
                {
                    // Obtener el ViewModel y activar el panel de edición
                    if (DataContext is GestionTareasViewModel viewModel)
                    {
                        viewModel.EditarTareaCommand.Execute(tarea);
                    }
                }
            }
        }
    }
}
