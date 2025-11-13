using System.Windows;
using System.Windows.Input;
using System.Linq;
using Hermes.Commands;
using Hermes.Views;

namespace Hermes.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        private string _nombreUsuarioActual = string.Empty;

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set => SetProperty(ref _nombreUsuarioActual, value);
        }

        public ICommand MostrarGestionEmpleadosCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public MainViewModel()
        {
            // Obtener usuario actual
            var usuario = App.UsuarioActual;
            if (usuario?.Empleado != null)
            {
                NombreUsuarioActual = $"{usuario.Empleado.NombresEmpleado} {usuario.Empleado.ApellidosEmpleado}";
            }

            // Comandos
            MostrarGestionEmpleadosCommand = new RelayCommand(_ => MostrarGestionEmpleados());
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());

            // Mostrar vista por defecto
            MostrarGestionEmpleados();
        }

        private void MostrarGestionEmpleados()
        {
            CurrentView = new GestionEmpleadosView();
        }

        private void CerrarSesion()
        {
            var result = MessageBox.Show("¿Esta seguro que desea cerrar sesion?",
                                         "Confirmar",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.UsuarioActual = null;

                var loginWindow = new LoginWindow();
                loginWindow.Show();

                Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
