using System.Windows;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class EditarEmpleadoWindow : Window
    {
        public EditarEmpleadoWindow(Empleado empleado)
        {
            InitializeComponent();
            DataContext = new EditarEmpleadoViewModel(empleado);
        }
    }
}
