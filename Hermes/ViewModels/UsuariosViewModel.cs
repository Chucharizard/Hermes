using System.Collections.ObjectModel;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class UsuariosViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;
        private ObservableCollection<Usuario> _usuarios = new();
        private Usuario? _usuarioSeleccionado;

        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        public Usuario? UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set => SetProperty(ref _usuarioSeleccionado, value);
        }

        public ICommand CargarUsuariosCommand { get; }
        public ICommand CrearUsuarioCommand { get; }
        public ICommand ActualizarUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }

        public UsuariosViewModel()
        {
            _usuarioService = new UsuarioService();
            Usuarios = new ObservableCollection<Usuario>();

            CargarUsuariosCommand = new RelayCommand(async _ => await CargarUsuariosAsync());
            CrearUsuarioCommand = new RelayCommand(_ => AbrirCrearUsuario());
            ActualizarUsuarioCommand = new RelayCommand(async _ => await ActualizarUsuarioAsync(), _ => UsuarioSeleccionado != null);
            EliminarUsuarioCommand = new RelayCommand(async _ => await EliminarUsuarioAsync(), _ => UsuarioSeleccionado != null);
        }

        private async Task CargarUsuariosAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }
        }

        private void AbrirCrearUsuario()
        {
            // Abrir ventana o dialog para crear usuario
        }

        private async Task ActualizarUsuarioAsync()
        {
            if (UsuarioSeleccionado != null)
            {
                await _usuarioService.ActualizarUsuarioAsync(UsuarioSeleccionado);
                await CargarUsuariosAsync();
            }
        }

        private async Task EliminarUsuarioAsync()
        {
            if (UsuarioSeleccionado != null)
            {
                await _usuarioService.EliminarUsuarioAsync(UsuarioSeleccionado.IdUsuario);
                await CargarUsuariosAsync();
            }
        }
    }
}
