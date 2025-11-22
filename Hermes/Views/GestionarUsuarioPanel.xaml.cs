using System.Windows;
using System.Windows.Controls;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class GestionarUsuarioPanel : UserControl
    {
        public GestionarUsuarioPanel()
        {
            InitializeComponent();
        }

        public void SetEmpleado(Empleado empleado)
        {
            var viewModel = new GestionarUsuarioViewModel(empleado);
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is GestionarUsuarioViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is GestionarUsuarioViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}
