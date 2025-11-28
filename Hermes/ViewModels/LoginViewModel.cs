using System.Windows;
using System.Windows.Input;
using System.Linq;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;
using System;

namespace Hermes.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AutenticacionService _autenticacionService;
        private readonly AuditoriaSesionService _auditoriaSesionService;
        private int _ciEmpleado;
        private string _password = string.Empty;
        private string _mensajeError = string.Empty;
        private bool _isLoading;
        private int _intentosFallidos;
        private int _intentosRestantes = 3;

        public int CiEmpleado
        {
            get => _ciEmpleado;
            set => SetProperty(ref _ciEmpleado, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int IntentosRestantes
        {
            get => _intentosRestantes;
            set => SetProperty(ref _intentosRestantes, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _autenticacionService = new AutenticacionService();
            _auditoriaSesionService = new AuditoriaSesionService();
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
        }

        private async Task LoginAsync()
        {
            MensajeError = string.Empty;

            if (CiEmpleado == 0 || string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Por favor, ingrese CI y contraseña";
                return;
            }

            IsLoading = true;

            var (success, usuario, mensaje) = await _autenticacionService.ValidarCredencialesAsync(CiEmpleado, Password);

            IsLoading = false;

            if (success && usuario != null)
            {
                // Guardar usuario en sesión
                App.UsuarioActual = usuario;

                // Cargar tema preferido del usuario
                if (!string.IsNullOrEmpty(usuario.TemaPreferido))
                {
                    ThemeService.Instance.ApplyTheme(usuario.TemaPreferido);
                }

                // Registrar auditoría de LOGIN
                try
                {
                    string nombreMaquina = Environment.MachineName;
                    await _auditoriaSesionService.RegistrarLoginAsync(
                        usuario.IdUsuario,
                        usuario.EmpleadoCi.ToString(),
                        nombreMaquina
                    );
                }
                catch (Exception ex)
                {
                    // Error en auditoría no debe bloquear el login
                    System.Diagnostics.Debug.WriteLine($"Error al registrar auditoría de login: {ex.Message}");
                }

                // Abrir ventana principal (todos los roles tienen acceso)
                // El menú se adapta automáticamente según el rol en MainViewModel
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Cerrar ventana de login
                Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.GetType().Name == "LoginWindow")?.Close();
            }
            else
            {
                // Incrementar intentos fallidos
                _intentosFallidos++;
                IntentosRestantes = 3 - _intentosFallidos;

                if (_intentosFallidos >= 3)
                {
                    MensajeError = "Ha excedido el número máximo de intentos. La aplicación se cerrará.";

                    // Esperar 2 segundos para que el usuario pueda leer el mensaje
                    await Task.Delay(2000);
                    CerrarAplicacion();
                }
                else
                {
                    MensajeError = $"{mensaje}\nIntentos restantes: {IntentosRestantes}";
                }
            }
        }

        private void CerrarAplicacion()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
    }
}
