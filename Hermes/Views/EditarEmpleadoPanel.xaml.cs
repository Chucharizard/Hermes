using System.Windows.Controls;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class EditarEmpleadoPanel : UserControl
    {
        public EditarEmpleadoPanel()
        {
            InitializeComponent();
        }

        public void SetEmpleado(Empleado empleado)
        {
            DataContext = new EditarEmpleadoViewModel(empleado);
        }
    }
}
