using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hermes.Helpers
{
    /// <summary>
    /// Convierte un string hexadecimal (ej: "#E74C3C") a un SolidColorBrush para WPF
    /// </summary>
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor)!;
                }
                catch
                {
                    return Brushes.Gray; // Color por defecto si hay error
                }
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
