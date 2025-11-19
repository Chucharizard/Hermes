using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Helpers;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class DiaCalendario : BaseViewModel
    {
        private DateTime _fecha;
        private string _indicador = "-";
        private bool _esSeleccionado;

        public DateTime Fecha
        {
            get => _fecha;
            set => SetProperty(ref _fecha, value);
        }

        public string DiaNombre => _fecha.ToString("ddd", new CultureInfo("es-ES")).Substring(0, 1).ToUpper();
        public int DiaNumero => _fecha.Day;

        public string Indicador
        {
            get => _indicador;
            set => SetProperty(ref _indicador, value);
        }

        public bool EsSeleccionado
        {
            get => _esSeleccionado;
            set => SetProperty(ref _esSeleccionado, value);
        }
    }

    public class GestionTareasViewModel : BaseViewModel
    {
        private readonly TareaService _tareaService;
        private readonly UsuarioService _usuarioService;
        private ObservableCollection<Tarea> _tareas = new();
        private ObservableCollection<Tarea> _tareasFiltradas = new();
        private Tarea? _tareaSeleccionada;
        private string _filtroTexto = string.Empty;
        private DateTime _semanaActual;
        private DateTime? _diaSeleccionado;

        // Propiedades editables
        private string _tituloEditable = string.Empty;
        private string _descripcionEditable = string.Empty;
        private string _estadoEditable = "Pendiente";
        private string _prioridadEditable = "Media";
        private DateTime _fechaInicioEditable = DateTime.Now;
        private DateTime _fechaLimiteEditable = DateTime.Now.AddDays(7);
        private Usuario? _usuarioReceptorEditable;

        public ObservableCollection<Tarea> Tareas
        {
            get => _tareas;
            set => SetProperty(ref _tareas, value);
        }

        public ObservableCollection<Tarea> TareasFiltradas
        {
            get => _tareasFiltradas;
            set => SetProperty(ref _tareasFiltradas, value);
        }

        public Tarea? TareaSeleccionada
        {
            get => _tareaSeleccionada;
            set
            {
                if (SetProperty(ref _tareaSeleccionada, value))
                {
                    CargarDatosEditables();
                }
            }
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set
            {
                SetProperty(ref _filtroTexto, value);
                FiltrarTareas();
            }
        }

        // Calendario
        public DateTime SemanaActual
        {
            get => _semanaActual;
            set
            {
                SetProperty(ref _semanaActual, value);
                ActualizarDiasSemana();
            }
        }

        public DateTime? DiaSeleccionado
        {
            get => _diaSeleccionado;
            set
            {
                SetProperty(ref _diaSeleccionado, value);
                FiltrarTareas();
            }
        }

        public string TituloSemana => $"Semana del {InicioSemana:dd/MM} al {FinSemana:dd/MM/yyyy}";

        public DateTime InicioSemana => _semanaActual.Date.AddDays(-(int)_semanaActual.DayOfWeek + (int)DayOfWeek.Monday);
        public DateTime FinSemana => InicioSemana.AddDays(6);

        // Días de la semana
        public ObservableCollection<DiaCalendario> DiasSemana { get; } = new();

        // Propiedades editables
        public string TituloEditable
        {
            get => _tituloEditable;
            set => SetProperty(ref _tituloEditable, value);
        }

        public string DescripcionEditable
        {
            get => _descripcionEditable;
            set => SetProperty(ref _descripcionEditable, value);
        }

        public string EstadoEditable
        {
            get => _estadoEditable;
            set => SetProperty(ref _estadoEditable, value);
        }

        public string PrioridadEditable
        {
            get => _prioridadEditable;
            set => SetProperty(ref _prioridadEditable, value);
        }

        public DateTime FechaInicioEditable
        {
            get => _fechaInicioEditable;
            set => SetProperty(ref _fechaInicioEditable, value);
        }

        public DateTime FechaLimiteEditable
        {
            get => _fechaLimiteEditable;
            set => SetProperty(ref _fechaLimiteEditable, value);
        }

        public Usuario? UsuarioReceptorEditable
        {
            get => _usuarioReceptorEditable;
            set => SetProperty(ref _usuarioReceptorEditable, value);
        }

        public string EmisorInfo => TareaSeleccionada?.UsuarioEmisor?.Empleado != null
            ? $"{TareaSeleccionada.UsuarioEmisor.Empleado.NombresEmpleado} {TareaSeleccionada.UsuarioEmisor.Empleado.ApellidosEmpleado}"
            : "No especificado";

        public ObservableCollection<Usuario> UsuariosDisponibles { get; } = new();

        public List<string> EstadosDisponibles { get; } = new()
        {
            "Pendiente",
            "Completado",
            "Vencido",
            "Observado",
            "Archivado"
        };

        public List<string> PrioridadesDisponibles { get; } = new()
        {
            "Baja",
            "Media",
            "Alta",
            "Urgente"
        };

        public int TotalTareas => Tareas?.Count ?? 0;
        public int TareasPendientes => Tareas?.Count(t => t.EstadoTarea == "Pendiente") ?? 0;
        public int TareasCompletadas => Tareas?.Count(t => t.EstadoTarea == "Completado") ?? 0;
        public int TareasVencidas => Tareas?.Count(t => t.EstadoTarea == "Vencido") ?? 0;

        public ICommand CargarTareasCommand { get; }
        public ICommand AbrirNuevaTareaCommand { get; }
        public ICommand EditarTareaCommand { get; }
        public ICommand EliminarTareaCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand SeleccionarDiaCommand { get; }
        public ICommand GuardarCambiosCommand { get; }

        public GestionTareasViewModel()
        {
            _tareaService = new TareaService();
            _usuarioService = new UsuarioService();
            Tareas = new ObservableCollection<Tarea>();
            TareasFiltradas = new ObservableCollection<Tarea>();
            _semanaActual = DateTime.Now;

            CargarTareasCommand = new RelayCommand(async _ => await CargarTareasAsync());
            AbrirNuevaTareaCommand = new RelayCommand(_ => AbrirNuevaTarea());
            EditarTareaCommand = new RelayCommand(tarea => EditarTarea(tarea as Tarea));
            EliminarTareaCommand = new RelayCommand(async tarea => await EliminarTareaAsync(tarea as Tarea));
            VerDetalleCommand = new RelayCommand(tarea => VerDetalle(tarea as Tarea));
            SemanaAnteriorCommand = new RelayCommand(_ => SemanaActual = SemanaActual.AddDays(-7));
            SemanaSiguienteCommand = new RelayCommand(_ => SemanaActual = SemanaActual.AddDays(7));
            SeleccionarDiaCommand = new RelayCommand(dia => SeleccionarDia(dia as DiaCalendario));
            GuardarCambiosCommand = new RelayCommand(async _ => await GuardarCambiosAsync(), _ => TareaSeleccionada != null);

            // Cargar datos iniciales
            ActualizarDiasSemana();
            Task.Run(async () =>
            {
                await CargarTareasAsync();
                await CargarUsuariosAsync();
            });
        }

        private async Task CargarTareasAsync()
        {
            // ⏰ ACTUALIZAR TAREAS VENCIDAS (al estilo Teams)
            await _tareaService.ActualizarTareasVencidasAsync();

            var tareas = await _tareaService.ObtenerTodasAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tareas.Clear();
                foreach (var tarea in tareas)
                {
                    Tareas.Add(tarea);
                }

                FiltrarTareas();
                OnPropertyChanged(nameof(TotalTareas));
                OnPropertyChanged(nameof(TareasPendientes));
                OnPropertyChanged(nameof(TareasCompletadas));
                OnPropertyChanged(nameof(TareasVencidas));
            });
        }

        private void FiltrarTareas()
        {
            TareasFiltradas.Clear();

            var filtradas = Tareas.AsEnumerable();

            // Filtrar por día seleccionado
            if (DiaSeleccionado.HasValue)
            {
                var diaInicio = DiaSeleccionado.Value.Date;
                var diaFin = diaInicio.AddDays(1);

                filtradas = filtradas.Where(t =>
                    (t.FechaInicioTarea >= diaInicio && t.FechaInicioTarea < diaFin) ||
                    (t.FechaLimiteTarea.HasValue && t.FechaLimiteTarea.Value >= diaInicio && t.FechaLimiteTarea.Value < diaFin) ||
                    (t.FechaCompletadaTarea.HasValue && t.FechaCompletadaTarea.Value >= diaInicio && t.FechaCompletadaTarea.Value < diaFin));
            }

            // Filtrar por texto
            if (!string.IsNullOrWhiteSpace(FiltroTexto))
            {
                filtradas = filtradas.Where(t =>
                    t.TituloTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    (t.DescripcionTarea?.ToLower().Contains(FiltroTexto.ToLower()) ?? false) ||
                    t.EstadoTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    t.PrioridadTarea.ToLower().Contains(FiltroTexto.ToLower()) ||
                    (t.UsuarioEmisor?.Empleado?.NombresEmpleado.ToLower().Contains(FiltroTexto.ToLower()) ?? false) ||
                    (t.UsuarioReceptor?.Empleado?.NombresEmpleado.ToLower().Contains(FiltroTexto.ToLower()) ?? false));
            }

            foreach (var tarea in filtradas)
            {
                TareasFiltradas.Add(tarea);
            }

            // Actualizar indicadores del calendario
            ActualizarIndicadoresCalendario();
        }

        private void AbrirNuevaTarea()
        {
            var ventana = new Views.NuevaTareaWindow();
            ventana.Owner = Application.Current.MainWindow;
            if (ventana.ShowDialog() == true)
            {
                Task.Run(async () => await CargarTareasAsync());
            }
        }

        private void EditarTarea(Tarea? tarea)
        {
            if (tarea == null) return;

            var ventana = new Views.EditarTareaWindow(tarea);
            ventana.Owner = Application.Current.MainWindow;
            if (ventana.ShowDialog() == true)
            {
                Task.Run(async () => await CargarTareasAsync());
            }
        }

        private async Task EliminarTareaAsync(Tarea? tarea)
        {
            if (tarea == null) return;

            // Usar DialogHelper para confirmación consistente
            if (!DialogHelper.ConfirmarEliminacion(tarea.TituloTarea))
                return;

            try
            {
                var eliminado = await _tareaService.EliminarAsync(tarea.IdTarea);

                if (eliminado)
                {
                    DialogHelper.MostrarExito("La tarea ha sido eliminada exitosamente.");
                    await CargarTareasAsync();
                    TareaSeleccionada = null;
                }
                else
                {
                    DialogHelper.MostrarError("No se pudo eliminar la tarea. Por favor, intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                DialogHelper.MostrarError($"Error al eliminar la tarea:\n{ex.Message}");
            }
        }

        private void VerDetalle(Tarea? tarea)
        {
            if (tarea == null) return;

            var emisor = tarea.UsuarioEmisor?.Empleado != null
                ? $"{tarea.UsuarioEmisor.Empleado.NombresEmpleado} {tarea.UsuarioEmisor.Empleado.ApellidosEmpleado}"
                : "No especificado";

            var receptor = tarea.UsuarioReceptor?.Empleado != null
                ? $"{tarea.UsuarioReceptor.Empleado.NombresEmpleado} {tarea.UsuarioReceptor.Empleado.ApellidosEmpleado}"
                : "No especificado";

            var fechaLimite = tarea.FechaLimiteTarea.HasValue
                ? tarea.FechaLimiteTarea.Value.ToString("dd/MM/yyyy")
                : "Sin fecha limite";

            var fechaCompletada = tarea.FechaCompletadaTarea.HasValue
                ? tarea.FechaCompletadaTarea.Value.ToString("dd/MM/yyyy HH:mm")
                : "No completada";

            MessageBox.Show(
                $"DETALLE DE TAREA\n\n" +
                $"Titulo: {tarea.TituloTarea}\n" +
                $"Descripcion: {tarea.DescripcionTarea ?? "Sin descripcion"}\n\n" +
                $"Estado: {tarea.EstadoTarea}\n" +
                $"Prioridad: {tarea.PrioridadTarea}\n\n" +
                $"Emisor: {emisor}\n" +
                $"Receptor: {receptor}\n\n" +
                $"Fecha Inicio: {tarea.FechaInicioTarea:dd/MM/yyyy}\n" +
                $"Fecha Limite: {fechaLimite}\n" +
                $"Fecha Completada: {fechaCompletada}",
                "Informacion de la Tarea",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task CargarUsuariosAsync()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                UsuariosDisponibles.Clear();
                foreach (var usuario in usuarios)
                {
                    UsuariosDisponibles.Add(usuario);
                }
            });
        }

        private void ActualizarDiasSemana()
        {
            DiasSemana.Clear();

            var inicioSemana = InicioSemana;

            for (int i = 0; i < 7; i++)
            {
                var dia = new DiaCalendario
                {
                    Fecha = inicioSemana.AddDays(i),
                    EsSeleccionado = false
                };
                DiasSemana.Add(dia);
            }

            ActualizarIndicadoresCalendario();
            OnPropertyChanged(nameof(TituloSemana));
        }

        private void ActualizarIndicadoresCalendario()
        {
            foreach (var dia in DiasSemana)
            {
                var diaInicio = dia.Fecha.Date;
                var diaFin = diaInicio.AddDays(1);

                var tareasDelDia = Tareas.Where(t =>
                    (t.FechaInicioTarea >= diaInicio && t.FechaInicioTarea < diaFin) ||
                    (t.FechaLimiteTarea.HasValue && t.FechaLimiteTarea.Value >= diaInicio && t.FechaLimiteTarea.Value < diaFin) ||
                    (t.FechaCompletadaTarea.HasValue && t.FechaCompletadaTarea.Value >= diaInicio && t.FechaCompletadaTarea.Value < diaFin)).ToList();

                if (!tareasDelDia.Any())
                {
                    dia.Indicador = "-";
                }
                else if (tareasDelDia.Any(t => t.EstadoTarea == "Completado"))
                {
                    dia.Indicador = "🟢";
                }
                else if (tareasDelDia.Any(t => t.EstadoTarea == "Vencido"))
                {
                    dia.Indicador = "⚪";
                }
                else
                {
                    dia.Indicador = "🔴";
                }
            }
        }

        private void SeleccionarDia(DiaCalendario? dia)
        {
            if (dia == null) return;

            // Deseleccionar todos los días
            foreach (var d in DiasSemana)
            {
                d.EsSeleccionado = false;
            }

            // Si se selecciona el mismo día, deseleccionar
            if (DiaSeleccionado == dia.Fecha.Date)
            {
                DiaSeleccionado = null;
            }
            else
            {
                dia.EsSeleccionado = true;
                DiaSeleccionado = dia.Fecha.Date;
            }
        }

        private void CargarDatosEditables()
        {
            if (TareaSeleccionada == null)
            {
                TituloEditable = string.Empty;
                DescripcionEditable = string.Empty;
                EstadoEditable = "Pendiente";
                PrioridadEditable = "Media";
                FechaInicioEditable = DateTime.Now;
                FechaLimiteEditable = DateTime.Now.AddDays(7);
                UsuarioReceptorEditable = null;
            }
            else
            {
                TituloEditable = TareaSeleccionada.TituloTarea;
                DescripcionEditable = TareaSeleccionada.DescripcionTarea ?? string.Empty;
                EstadoEditable = TareaSeleccionada.EstadoTarea;
                PrioridadEditable = TareaSeleccionada.PrioridadTarea;
                FechaInicioEditable = TareaSeleccionada.FechaInicioTarea;
                FechaLimiteEditable = TareaSeleccionada.FechaLimiteTarea ?? DateTime.Now.AddDays(7);
                UsuarioReceptorEditable = TareaSeleccionada.UsuarioReceptor;
            }

            OnPropertyChanged(nameof(EmisorInfo));
        }

        private async Task GuardarCambiosAsync()
        {
            if (TareaSeleccionada == null) return;

            // Validaciones
            if (string.IsNullOrWhiteSpace(TituloEditable))
            {
                DialogHelper.MostrarError("El título es obligatorio.");
                return;
            }

            if (UsuarioReceptorEditable == null)
            {
                DialogHelper.MostrarError("Debe seleccionar un usuario receptor.");
                return;
            }

            if (FechaLimiteEditable < FechaInicioEditable)
            {
                DialogHelper.MostrarError("La fecha límite no puede ser anterior a la fecha de inicio.");
                return;
            }

            try
            {
                // Actualizar la tarea con los valores editados
                TareaSeleccionada.TituloTarea = TituloEditable;
                TareaSeleccionada.DescripcionTarea = DescripcionEditable;
                TareaSeleccionada.EstadoTarea = EstadoEditable;
                TareaSeleccionada.PrioridadTarea = PrioridadEditable;
                TareaSeleccionada.FechaInicioTarea = FechaInicioEditable;
                TareaSeleccionada.FechaLimiteTarea = FechaLimiteEditable;
                TareaSeleccionada.UsuarioReceptorId = UsuarioReceptorEditable.IdUsuario;
                TareaSeleccionada.UsuarioReceptor = UsuarioReceptorEditable;

                var actualizado = await _tareaService.ActualizarAsync(TareaSeleccionada);

                if (actualizado)
                {
                    DialogHelper.MostrarExito("Los cambios se han guardado exitosamente.");
                    await CargarTareasAsync();
                }
                else
                {
                    DialogHelper.MostrarError("No se pudieron guardar los cambios. Por favor, intente nuevamente.");
                }
            }
            catch (Exception ex)
            {
                DialogHelper.MostrarError($"Error al guardar los cambios:\n{ex.Message}");
            }
        }
    }
}
