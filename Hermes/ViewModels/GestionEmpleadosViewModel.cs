using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class GestionEmpleadosViewModel : BaseViewModel
    {
        private readonly EmpleadoService _empleadoService;
        private ObservableCollection<Empleado> _empleados = new();
        private ObservableCollection<Empleado> _empleadosFiltrados = new();
        private Empleado? _empleadoSeleccionado;
        private string _filtroTexto = string.Empty;
        private string _filtroEstado = "Todos";
        private string? _accionActual;
        private Empleado? _empleadoEnAccion;

        public ObservableCollection<Empleado> Empleados
        {
            get => _empleados;
            set => SetProperty(ref _empleados, value);
        }

        public ObservableCollection<Empleado> EmpleadosFiltrados
        {
            get => _empleadosFiltrados;
            set => SetProperty(ref _empleadosFiltrados, value);
        }

        public Empleado? EmpleadoSeleccionado
        {
            get => _empleadoSeleccionado;
            set => SetProperty(ref _empleadoSeleccionado, value);
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set
            {
                SetProperty(ref _filtroTexto, value);
                FiltrarEmpleados();
            }
        }

        public string FiltroEstado
        {
            get => _filtroEstado;
            set
            {
                SetProperty(ref _filtroEstado, value);
                FiltrarEmpleados();
            }
        }

        public string? AccionActual
        {
            get => _accionActual;
            set
            {
                SetProperty(ref _accionActual, value);
                ActualizarViewModelAccion();
            }
        }

        public Empleado? EmpleadoEnAccion
        {
            get => _empleadoEnAccion;
            set
            {
                SetProperty(ref _empleadoEnAccion, value);
                ActualizarViewModelAccion();
            }
        }

        private object? _viewModelAccion;
        public object? ViewModelAccion
        {
            get => _viewModelAccion;
            set => SetProperty(ref _viewModelAccion, value);
        }

        private void ActualizarViewModelAccion()
        {
            if (AccionActual == null)
            {
                ViewModelAccion = null;
                return;
            }

            if (EmpleadoEnAccion == null && AccionActual != "Nuevo")
            {
                ViewModelAccion = null;
                return;
            }

            ViewModelAccion = AccionActual switch
            {
                "Nuevo" => new NuevoEmpleadoViewModel(),
                "Editar" when EmpleadoEnAccion != null => new EditarEmpleadoViewModel(EmpleadoEnAccion),
                "Usuario" when EmpleadoEnAccion != null => new GestionarUsuarioViewModel(EmpleadoEnAccion),
                "VerDetalle" when EmpleadoEnAccion != null => new VerDetalleEmpleadoViewModel(EmpleadoEnAccion),
                _ => null
            };
        }

        // Lista de opciones para el filtro de estado
        public List<string> OpcionesFiltroEstado { get; } = new()
        {
            "Todos",
            "Activos",
            "Inactivos"
        };

        public int TotalEmpleados => Empleados?.Count ?? 0;
        public int EmpleadosActivos => Empleados?.Count(e => e.EsActivoEmpleado) ?? 0;
        public int EmpleadosInactivos => Empleados?.Count(e => !e.EsActivoEmpleado) ?? 0;

        public ICommand CargarEmpleadosCommand { get; }
        public ICommand AbrirNuevoEmpleadoCommand { get; }
        public ICommand EditarEmpleadoCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand GestionarUsuarioCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand CerrarAccionCommand { get; }

        public GestionEmpleadosViewModel()
        {
            _empleadoService = new EmpleadoService();
            Empleados = new ObservableCollection<Empleado>();
            EmpleadosFiltrados = new ObservableCollection<Empleado>();

            CargarEmpleadosCommand = new RelayCommand(async _ => await CargarEmpleadosAsync());
            AbrirNuevoEmpleadoCommand = new RelayCommand(_ => AbrirNuevoEmpleado());
            EditarEmpleadoCommand = new RelayCommand(empleado => EditarEmpleado(empleado as Empleado));
            VerDetalleCommand = new RelayCommand(empleado => VerDetalle(empleado as Empleado));
            GestionarUsuarioCommand = new RelayCommand(empleado => GestionarUsuario(empleado as Empleado));
            VolverCommand = new RelayCommand(_ => Volver());
            CerrarAccionCommand = new RelayCommand(_ => CerrarAccion());

            // Cargar datos iniciales
            Task.Run(async () => await CargarEmpleadosAsync());
        }

        private async Task CargarEmpleadosAsync()
        {
            var empleados = await _empleadoService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Empleados.Clear();
                foreach (var empleado in empleados)
                {
                    Empleados.Add(empleado);
                }

                FiltrarEmpleados();
                OnPropertyChanged(nameof(TotalEmpleados));
                OnPropertyChanged(nameof(EmpleadosActivos));
                OnPropertyChanged(nameof(EmpleadosInactivos));
            });
        }

        private void FiltrarEmpleados()
        {
            EmpleadosFiltrados.Clear();

            // Aplicar filtro de texto (CI, nombre, apellido)
            var filtrados = string.IsNullOrWhiteSpace(FiltroTexto)
                ? Empleados
                : Empleados.Where(e =>
                    e.CiEmpleado.ToString().Contains(FiltroTexto) ||
                    e.NombresEmpleado.ToLower().Contains(FiltroTexto.ToLower()) ||
                    e.ApellidosEmpleado.ToLower().Contains(FiltroTexto.ToLower()));

            // Aplicar filtro de estado (Todos/Activos/Inactivos)
            filtrados = FiltroEstado switch
            {
                "Activos" => filtrados.Where(e => e.EsActivoEmpleado),
                "Inactivos" => filtrados.Where(e => !e.EsActivoEmpleado),
                _ => filtrados // "Todos"
            };

            foreach (var empleado in filtrados)
            {
                EmpleadosFiltrados.Add(empleado);
            }
        }

        private void AbrirNuevoEmpleado()
        {
            EmpleadoEnAccion = new Empleado { EsActivoEmpleado = true };
            AccionActual = "Nuevo";
        }

        private void EditarEmpleado(Empleado? empleado)
        {
            if (empleado == null) return;
            EmpleadoEnAccion = empleado;
            AccionActual = "Editar";
        }

        private void VerDetalle(Empleado? empleado)
        {
            if (empleado == null) return;
            EmpleadoEnAccion = empleado;
            AccionActual = "VerDetalle";
        }

        private void GestionarUsuario(Empleado? empleado)
        {
            if (empleado == null) return;
            EmpleadoEnAccion = empleado;
            AccionActual = "Usuario";
        }

        private void Volver()
        {
            AccionActual = null;
            EmpleadoEnAccion = null;
            Task.Run(async () => await CargarEmpleadosAsync());
        }

        private void CerrarAccion()
        {
            AccionActual = null;
            EmpleadoEnAccion = null;
        }
    }
}
