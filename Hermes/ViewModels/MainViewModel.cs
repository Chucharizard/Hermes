using System.Windows;
using System.Windows.Input;
using System.Linq;
using Hermes.Commands;
using Hermes.Views;
using Hermes.Services;
using System;

namespace Hermes.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        private string _nombreUsuarioActual = string.Empty;
        private string _rolUsuario = string.Empty;
        private bool _puedeEnviarTareas = false;
        private bool _esAdministrador = false;
        private bool _sidebarColapsado = false;
        private double _anchoSidebar = 250;

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

        public string RolUsuario
        {
            get => _rolUsuario;
            set => SetProperty(ref _rolUsuario, value);
        }

        // Propiedades para controlar visibilidad del menú según rol
        public bool PuedeEnviarTareas
        {
            get => _puedeEnviarTareas;
            set => SetProperty(ref _puedeEnviarTareas, value);
        }

        public bool EsAdministrador
        {
            get => _esAdministrador;
            set => SetProperty(ref _esAdministrador, value);
        }

        // Propiedades para sidebar colapsable
        public bool SidebarColapsado
        {
            get => _sidebarColapsado;
            set
            {
                SetProperty(ref _sidebarColapsado, value);
                AnchoSidebar = value ? 70 : 250;
            }
        }

        public double AnchoSidebar
        {
            get => _anchoSidebar;
            set => SetProperty(ref _anchoSidebar, value);
        }

        public ICommand MostrarGestionEmpleadosCommand { get; }
        public ICommand MostrarGestionTareasCommand { get; }
        public ICommand MostrarDashboardCommand { get; }
        public ICommand MostrarBandejaTareasEnviadasCommand { get; }
        public ICommand MostrarBandejaTareasRecibidasCommand { get; }
        public ICommand MostrarAuditoriaCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand ToggleSidebarCommand { get; }

        public MainViewModel()
        {
            // Obtener usuario actual
            var usuario = App.UsuarioActual;
            if (usuario?.Empleado != null)
            {
                NombreUsuarioActual = $"{usuario.Empleado.NombresEmpleado} {usuario.Empleado.ApellidosEmpleado}";
            }

            // Configurar permisos según el rol
            if (usuario?.Rol != null)
            {
                RolUsuario = usuario.Rol.NombreRol;

                // Solo Broker, Secretaria y Abogada/Abogado pueden enviar tareas
                PuedeEnviarTareas = RolUsuario.Equals("Broker", StringComparison.OrdinalIgnoreCase) ||
                                   RolUsuario.Equals("Secretaria", StringComparison.OrdinalIgnoreCase) ||
                                   RolUsuario.Equals("Abogada", StringComparison.OrdinalIgnoreCase) ||
                                   RolUsuario.Equals("Abogado", StringComparison.OrdinalIgnoreCase);

                // Solo Broker tiene acceso completo a administración
                EsAdministrador = RolUsuario.Equals("Broker", StringComparison.OrdinalIgnoreCase);
            }

            // Comandos
            MostrarGestionEmpleadosCommand = new RelayCommand(_ => MostrarGestionEmpleados());
            MostrarGestionTareasCommand = new RelayCommand(_ => MostrarGestionTareas());
            MostrarDashboardCommand = new RelayCommand(_ => MostrarDashboard());
            MostrarBandejaTareasEnviadasCommand = new RelayCommand(_ => MostrarBandejaTareasEnviadas());
            MostrarBandejaTareasRecibidasCommand = new RelayCommand(_ => MostrarBandejaTareasRecibidas());
            MostrarAuditoriaCommand = new RelayCommand(_ => MostrarAuditoria());
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());
            ToggleSidebarCommand = new RelayCommand(_ => ToggleSidebar());

            // Mostrar vista por defecto según el rol
            if (PuedeEnviarTareas || EsAdministrador)
            {
                MostrarDashboard();
            }
            else
            {
                // Para asesores y administrativos, mostrar directamente sus tareas recibidas
                MostrarBandejaTareasRecibidas();
            }
        }

        private void MostrarGestionEmpleados()
        {
            CurrentView = new GestionEmpleadosView();
        }

        private void MostrarGestionTareas()
        {
            CurrentView = new GestionTareasView();
        }

        private void MostrarDashboard()
        {
            CurrentView = new DashboardView();
        }

        private void MostrarBandejaTareasEnviadas()
        {
            CurrentView = new BandejaTareasEnviadasView();
        }

        private void MostrarBandejaTareasRecibidas()
        {
            CurrentView = new BandejaTareasRecibidasView();
        }

        private void MostrarAuditoria()
        {
            CurrentView = new AuditoriaView();
        }

        public void MostrarBandejaTareasRecibidasConFiltro(string filtroEstado)
        {
            var view = new BandejaTareasRecibidasView();
            if (view.DataContext is BandejaTareasRecibidasViewModel viewModel)
            {
                viewModel.FiltroEstado = filtroEstado == "Total" ? "Todas" : filtroEstado;
            }
            CurrentView = view;
        }

        private void ToggleSidebar()
        {
            SidebarColapsado = !SidebarColapsado;
        }

        private async void CerrarSesion()
        {
            var result = MessageBox.Show("�Esta seguro que desea cerrar sesion?",
                                         "Confirmar",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Registrar auditoría de LOGOUT antes de cerrar sesión
                if (App.UsuarioActual != null)
                {
                    try
                    {
                        var auditoriaSesionService = new AuditoriaSesionService();
                        string nombreMaquina = Environment.MachineName;
                        await auditoriaSesionService.RegistrarLogoutAsync(
                            App.UsuarioActual.IdUsuario,
                            App.UsuarioActual.EmpleadoCi,
                            nombreMaquina
                        );
                    }
                    catch (Exception ex)
                    {
                        // Error en auditoría no debe bloquear el logout
                        System.Diagnostics.Debug.WriteLine($"Error al registrar auditoría de logout: {ex.Message}");
                    }
                }

                App.UsuarioActual = null;

                var loginWindow = new LoginWindow();
                loginWindow.Show();

                Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
