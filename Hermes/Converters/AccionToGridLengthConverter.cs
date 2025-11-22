using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte la presencia de una acción a GridLength para pantalla dividida
    /// Sin acción: Lista 100%, Panel oculto
    /// Con acción: Lista 45%, Separador 10px, Panel 55%
    /// Parámetros: "Lista", "Separador", "Detalle"
    /// </summary>
    public class AccionToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hayAccion = value != null;
            string columna = parameter?.ToString() ?? "Lista";

            return columna switch
            {
                // Sin acción: Lista ocupa todo | Con acción: Lista 45%
                "Lista" => hayAccion
                    ? new GridLength(0.45, GridUnitType.Star)
                    : new GridLength(1, GridUnitType.Star),

                // Separador: 0 sin acción, 10px con acción
                "Separador" => hayAccion
                    ? new GridLength(10, GridUnitType.Pixel)
                    : new GridLength(0, GridUnitType.Pixel),

                // Sin acción: Panel oculto | Con acción: Panel 55%
                "Detalle" => hayAccion
                    ? new GridLength(0.55, GridUnitType.Star)
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
