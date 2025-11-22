using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte la presencia de una tarea seleccionada a GridLength
    /// Parámetros: "Lista", "Separador", "Detalle"
    /// </summary>
    public class TareaSeleccionadaToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool haySeleccion = value != null;
            string columna = parameter?.ToString() ?? "Lista";

            return columna switch
            {
                // Sin selección: Lista ocupa todo | Con selección: Lista desaparece
                "Lista" => haySeleccion
                    ? new GridLength(0, GridUnitType.Pixel)
                    : new GridLength(1, GridUnitType.Star),

                // Separador siempre 0 (no necesario)
                "Separador" => new GridLength(0, GridUnitType.Pixel),

                // Sin selección: Detalle no visible | Con selección: Detalle ocupa todo
                "Detalle" => haySeleccion
                    ? new GridLength(1, GridUnitType.Star)
                    : new GridLength(0, GridUnitType.Pixel),

                _ => new GridLength(1, GridUnitType.Star)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
