using System.Windows;
using System.Windows.Input;
using Hermes.Commands;
using Hermes.Models;
using Hermes.Services;

namespace Hermes.ViewModels
{
    public class EditarEmpleadoViewModel : BaseViewModel
    {
        private readonly EmpleadoService _empleadoService;
        private Empleado _empleado = new();
        private Empleado _empleadoOriginal = new();
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

        public ICommand ActualizarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarEmpleadoViewModel(Empleado empleadoAEditar)
        {
            _empleadoService = new EmpleadoService();

            // Guardar una copia del empleado original
            _empleadoOriginal = empleadoAEditar;

            // Crear una copia para editar
            Empleado = new Empleado
            {
                CiEmpleado = empleadoAEditar.CiEmpleado,
                NombresEmpleado = empleadoAEditar.NombresEmpleado,
                ApellidosEmpleado = empleadoAEditar.ApellidosEmpleado,
                TelefonoEmpleado = empleadoAEditar.TelefonoEmpleado,
                CorreoEmpleado = empleadoAEditar.CorreoEmpleado,
                EsActivoEmpleado = empleadoAEditar.EsActivoEmpleado
            };

            ActualizarCommand = new RelayCommand(async _ => await ActualizarAsync());
            CancelarCommand = new RelayCommand(_ => Cancelar());
        }

        private async Task ActualizarAsync()
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

            // Actualizar
            var resultado = await _empleadoService.ActualizarAsync(Empleado);

            if (resultado)
            {
                // Actualizar el objeto original en memoria para que la UI se actualice inmediatamente
                _empleadoOriginal.NombresEmpleado = Empleado.NombresEmpleado;
                _empleadoOriginal.ApellidosEmpleado = Empleado.ApellidosEmpleado;
                _empleadoOriginal.TelefonoEmpleado = Empleado.TelefonoEmpleado;
                _empleadoOriginal.CorreoEmpleado = Empleado.CorreoEmpleado;
                _empleadoOriginal.EsActivoEmpleado = Empleado.EsActivoEmpleado;

                MessageBox.Show("Empleado actualizado exitosamente",
                              "Exito",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // Cerrar la ventana con resultado positivo
                var ventana = Application.Current.Windows.OfType<Views.EditarEmpleadoWindow>().FirstOrDefault();
                if (ventana != null)
                {
                    ventana.DialogResult = true;
                    ventana.Close();
                }
            }
            else
            {
                MensajeError = "Error al actualizar el empleado";
            }
        }

        private void Cancelar()
        {
            var ventana = Application.Current.Windows.OfType<Views.EditarEmpleadoWindow>().FirstOrDefault();
            if (ventana != null)
            {
                ventana.DialogResult = false;
                ventana.Close();
            }
        }
    }
}
