using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hermes.Converters
{
    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mostrarPanel = value is bool b && b;
            var columna = parameter as string;

            return columna switch
            {
                "Lista" => mostrarPanel ? new GridLength(0.6, GridUnitType.Star) : new GridLength(1, GridUnitType.Star),
                "Separador" => mostrarPanel ? new GridLength(10, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel),
                "Panel" => mostrarPanel ? new GridLength(0.4, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel),
                _ => new GridLength(1, GridUnitType.Star)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
