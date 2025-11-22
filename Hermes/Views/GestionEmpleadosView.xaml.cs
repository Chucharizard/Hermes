using System.Windows.Controls;
using System.Windows.Input;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    /// <summary>
    /// Interaction logic for GestionEmpleadosView.xaml
    /// </summary>
    public partial class GestionEmpleadosView : UserControl
    {
        public GestionEmpleadosView()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Obtener el DataGrid
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // Obtener el empleado seleccionado
            var empleado = dataGrid.SelectedItem as Empleado;
            if (empleado == null) return;

            // Obtener el ViewModel y ejecutar el comando Ver Detalle
            var viewModel = DataContext as GestionEmpleadosViewModel;
            if (viewModel != null && viewModel.VerDetalleCommand.CanExecute(empleado))
            {
                viewModel.VerDetalleCommand.Execute(empleado);
            }
        }
    }
}
