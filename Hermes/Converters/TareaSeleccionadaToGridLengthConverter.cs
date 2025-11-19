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
                "Lista" => haySeleccion
                    ? new GridLength(400, GridUnitType.Pixel)
                    : new GridLength(1, GridUnitType.Star),

                "Separador" => haySeleccion
                    ? new GridLength(12, GridUnitType.Pixel)
                    : new GridLength(0, GridUnitType.Pixel),

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
