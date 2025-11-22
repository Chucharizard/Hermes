using Hermes.Models;

namespace Hermes.ViewModels
{
    public class VerDetalleEmpleadoViewModel : BaseViewModel
    {
        private Empleado _empleado;

        public Empleado Empleado
        {
            get => _empleado;
            set => SetProperty(ref _empleado, value);
        }

        public VerDetalleEmpleadoViewModel(Empleado empleado)
        {
            _empleado = empleado;
        }
    }
}
