using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;
using Hermes.Helpers;

namespace Hermes.ViewModels
{
    public class GestionarUsuarioViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;
        private readonly RolService _rolService;
        private Empleado _empleado = new();
        private Usuario _usuario = new();
        private ObservableCollection<Rol> _roles = new();
        private Rol? _rolSeleccionado;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _mensajeError = string.Empty;
        private string _mensajeExito = string.Empty;
        private bool _esNuevoUsuario;

        public Empleado Empleado
        {
            get => _empleado;
            set => SetProperty(ref _empleado, value);
        }

        public Usuario Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        public ObservableCollection<Rol> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public Rol? RolSeleccionado
        {
            get => _rolSeleccionado;
            set => SetProperty(ref _rolSeleccionado, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public string MensajeExito
        {
            get => _mensajeExito;
            set => SetProperty(ref _mensajeExito, value);
        }

        public string EmpleadoNombreCompleto => $"{Empleado.NombresEmpleado} {Empleado.ApellidosEmpleado}";

        public string TextoBotonGuardar => _esNuevoUsuario ? "Crear Usuario" : "Actualizar Usuario";

        public ICommand GuardarUsuarioCommand { get; }
        public ICommand CancelarCommand { get; }

        public GestionarUsuarioViewModel(Empleado empleado)
        {
            _usuarioService = new UsuarioService();
            _rolService = new RolService();
            Empleado = empleado;
            Roles = new ObservableCollection<Rol>();

            GuardarUsuarioCommand = new RelayCommand(async _ => await GuardarUsuarioAsync());
            CancelarCommand = new RelayCommand(_ => Cancelar());

            Task.Run(async () => await InicializarAsync());
        }

        private async Task InicializarAsync()
        {
            // Cargar roles
            var roles = await _rolService.ObtenerActivosAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var rol in roles)
                {
                    Roles.Add(rol);
                }
            });

            // Verificar si el empleado ya tiene usuario
            var usuarioExistente = await _usuarioService.ObtenerPorEmpleadoCiAsync(Empleado.CiEmpleado);

            if (usuarioExistente != null)
            {
                _esNuevoUsuario = false;
                Usuario = usuarioExistente;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    RolSeleccionado = Roles.FirstOrDefault(r => r.IdRol == Usuario.RolId);
                    OnPropertyChanged(nameof(TextoBotonGuardar));
                    OnPropertyChanged(nameof(EmpleadoNombreCompleto));
                });
            }
            else
            {
                _esNuevoUsuario = true;
                Usuario = new Usuario
                {
                    EmpleadoCi = Empleado.CiEmpleado,
                    EsActivoUsuario = true
                };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(TextoBotonGuardar));
                    OnPropertyChanged(nameof(EmpleadoNombreCompleto));
                });
            }
        }

        private async Task GuardarUsuarioAsync()
        {
            MensajeError = string.Empty;
            MensajeExito = string.Empty;

            // Validaciones
            if (string.IsNullOrWhiteSpace(Usuario.NombreUsuario))
            {
                MensajeError = "El nombre de usuario es obligatorio";
                return;
            }

            if (_esNuevoUsuario || !string.IsNullOrWhiteSpace(Password))
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    MensajeError = "La contrase�a es obligatoria";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    MensajeError = "Las contrase�as no coinciden";
                    return;
                }

                if (Password.Length < 6)
                {
                    MensajeError = "La contrase�a debe tener al menos 6 caracteres";
                    return;
                }
            }

            if (RolSeleccionado == null)
            {
                MensajeError = "Debe seleccionar un rol";
                return;
            }

            // Asignar valores
            Usuario.RolId = RolSeleccionado.IdRol;

            // Si se proporciona contraseña, hashearla usando BCrypt
            if (!string.IsNullOrWhiteSpace(Password))
            {
                Usuario.PasswordUsuario = PasswordHasher.HashPassword(Password);
            }

            // Guardar
            bool resultado;
            if (_esNuevoUsuario)
            {
                resultado = await _usuarioService.CrearAsync(Usuario);
            }
            else
            {
                resultado = await _usuarioService.ActualizarAsync(Usuario);
            }

            if (resultado)
            {
                MensajeExito = _esNuevoUsuario ? "Usuario creado exitosamente" : "Usuario actualizado exitosamente";

                await Task.Delay(1500);

                Application.Current.Windows.OfType<Views.GestionarUsuarioWindow>().FirstOrDefault()?.Close();
            }
            else
            {
                MensajeError = "Error al guardar el usuario";
            }
        }

        private void Cancelar()
        {
            Application.Current.Windows.OfType<Views.GestionarUsuarioWindow>().FirstOrDefault()?.Close();
        }
    }
}
