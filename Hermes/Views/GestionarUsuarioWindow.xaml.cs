using System.Windows;
using System.Windows.Controls;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    /// <summary>
    /// Interaction logic for GestionarUsuarioWindow.xaml
    /// </summary>
    public partial class GestionarUsuarioWindow : Window
    {
        public GestionarUsuarioWindow(Empleado empleado)
        {
            InitializeComponent();
            DataContext = new GestionarUsuarioViewModel(empleado);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && sender is PasswordBox passwordBox)
            {
                ((GestionarUsuarioViewModel)this.DataContext).Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && sender is PasswordBox passwordBox)
            {
                ((GestionarUsuarioViewModel)this.DataContext).ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
