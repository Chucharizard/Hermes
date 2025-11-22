using System.Linq;
using System.Windows;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class VerDetalleEmpleadoViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;
        private Empleado _empleado;
        private Usuario? _usuario;
        private bool _tieneUsuario;

        public Empleado Empleado
        {
            get => _empleado;
            set => SetProperty(ref _empleado, value);
        }

        public Usuario? Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        public bool TieneUsuario
        {
            get => _tieneUsuario;
            set => SetProperty(ref _tieneUsuario, value);
        }

        public VerDetalleEmpleadoViewModel(Empleado empleado)
        {
            _empleado = empleado;
            _usuarioService = new UsuarioService();

            // Cargar usuario asociado
            Task.Run(async () => await CargarUsuarioAsync());
        }

        private async Task CargarUsuarioAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Usuario = usuarios.FirstOrDefault(u => u.EmpleadoCi == Empleado.CiEmpleado);
                TieneUsuario = Usuario != null;
            });
        }
    }
}
