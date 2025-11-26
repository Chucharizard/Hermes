using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class NuevoEmpleadoViewModel : BaseViewModel
    {
        private readonly EmpleadoService _empleadoService;
        private readonly Action? _onSaveCompleted;
        private Empleado _empleado = new();
        private string _mensajeError = string.Empty;

        public Empleado Empleado
        {
            get => _empleado;
            set => SetProperty(ref _empleado, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public ICommand GuardarCommand { get; }

        public NuevoEmpleadoViewModel(Action? onSaveCompleted = null)
        {
            _empleadoService = new EmpleadoService();
            _onSaveCompleted = onSaveCompleted;
            Empleado = new Empleado
            {
                EsActivoEmpleado = true
            };

            GuardarCommand = new RelayCommand(async _ => await GuardarAsync());
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;

            // Validaciones
            if (Empleado.CiEmpleado <= 0)
            {
                MensajeError = "El CI debe ser un numero valido";
                return;
            }

            if (string.IsNullOrWhiteSpace(Empleado.NombresEmpleado))
            {
                MensajeError = "Los nombres son obligatorios";
                return;
            }

            if (string.IsNullOrWhiteSpace(Empleado.ApellidosEmpleado))
            {
                MensajeError = "Los apellidos son obligatorios";
                return;
            }

            // Verificar si ya existe
            var existe = await _empleadoService.ObtenerPorCiAsync(Empleado.CiEmpleado);
            if (existe != null)
            {
                MensajeError = "Ya existe un empleado con ese CI";
                return;
            }

            // Guardar
            var resultado = await _empleadoService.CrearAsync(Empleado);

            if (resultado)
            {
                MessageBox.Show("Empleado registrado exitosamente",
                              "Exito",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // Notificar al padre que la operación se completó (cerrar panel y recargar lista)
                _onSaveCompleted?.Invoke();
            }
            else
            {
                MensajeError = "Error al guardar el empleado";
            }
        }
    }
}
