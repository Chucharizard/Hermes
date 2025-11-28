using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class AuditoriaViewModel : BaseViewModel
    {
        private readonly AuditoriaSesionService _auditoriaSesionService;
        private readonly AuditoriaEmpleadoUsuarioService _auditoriaEmpleadoUsuarioService;
        private readonly AuditoriaTareaService _auditoriaTareaService;

        private ObservableCollection<AuditoriaSesion> _auditoriaSesion;
        private ObservableCollection<AuditoriaEmpleadoUsuario> _auditoriaEmpleadoUsuario;
        private ObservableCollection<AuditoriaTarea> _auditoriaTarea;

        private DateTime _fechaInicio = DateTime.Now.AddDays(-7);
        private DateTime _fechaFin = DateTime.Now;
        private bool _isLoading;

        public ObservableCollection<AuditoriaSesion> AuditoriaSesion
        {
            get => _auditoriaSesion;
            set => SetProperty(ref _auditoriaSesion, value);
        }

        public ObservableCollection<AuditoriaEmpleadoUsuario> AuditoriaEmpleadoUsuario
        {
            get => _auditoriaEmpleadoUsuario;
            set => SetProperty(ref _auditoriaEmpleadoUsuario, value);
        }

        public ObservableCollection<AuditoriaTarea> AuditoriaTarea
        {
            get => _auditoriaTarea;
            set => SetProperty(ref _auditoriaTarea, value);
        }

        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set => SetProperty(ref _fechaInicio, value);
        }

        public DateTime FechaFin
        {
            get => _fechaFin;
            set => SetProperty(ref _fechaFin, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CargarDatosCommand { get; }
        public ICommand FiltrarPorFechaCommand { get; }

        public AuditoriaViewModel()
        {
            _auditoriaSesionService = new AuditoriaSesionService();
            _auditoriaEmpleadoUsuarioService = new AuditoriaEmpleadoUsuarioService();
            _auditoriaTareaService = new AuditoriaTareaService();

            _auditoriaSesion = new ObservableCollection<AuditoriaSesion>();
            _auditoriaEmpleadoUsuario = new ObservableCollection<AuditoriaEmpleadoUsuario>();
            _auditoriaTarea = new ObservableCollection<AuditoriaTarea>();

            CargarDatosCommand = new RelayCommand(async _ => await CargarDatosAsync());
            FiltrarPorFechaCommand = new RelayCommand(async _ => await FiltrarPorFechaAsync());

            // Cargar datos iniciales
            Task.Run(async () => await CargarDatosAsync());
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                IsLoading = true;

                // Cargar auditoría de sesiones (últimos 7 días)
                var sesiones = await _auditoriaSesionService.ObtenerPorFechasAsync(
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaSesion.Clear();
                    foreach (var item in sesiones)
                    {
                        AuditoriaSesion.Add(item);
                    }
                });

                // Cargar auditoría de empleados/usuarios (últimos 7 días)
                var empleadosUsuarios = await _auditoriaEmpleadoUsuarioService.ObtenerPorFechasAsync(
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaEmpleadoUsuario.Clear();
                    foreach (var item in empleadosUsuarios)
                    {
                        AuditoriaEmpleadoUsuario.Add(item);
                    }
                });

                // Cargar auditoría de tareas (últimos 7 días)
                var tareas = await _auditoriaTareaService.ObtenerPorFechasAsync(
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaTarea.Clear();
                    foreach (var item in tareas)
                    {
                        AuditoriaTarea.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error al cargar datos de auditoría: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FiltrarPorFechaAsync()
        {
            if (FechaInicio > FechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha de fin.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                // Filtrar auditoría de sesiones
                var sesiones = await _auditoriaSesionService.ObtenerPorFechasAsync(FechaInicio, FechaFin);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaSesion.Clear();
                    foreach (var item in sesiones)
                    {
                        AuditoriaSesion.Add(item);
                    }
                });

                // Filtrar auditoría de empleados/usuarios
                var empleadosUsuarios = await _auditoriaEmpleadoUsuarioService.ObtenerPorFechasAsync(FechaInicio, FechaFin);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaEmpleadoUsuario.Clear();
                    foreach (var item in empleadosUsuarios)
                    {
                        AuditoriaEmpleadoUsuario.Add(item);
                    }
                });

                // Filtrar auditoría de tareas
                var tareas = await _auditoriaTareaService.ObtenerPorFechasAsync(FechaInicio, FechaFin);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuditoriaTarea.Clear();
                    foreach (var item in tareas)
                    {
                        AuditoriaTarea.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error al filtrar datos: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
