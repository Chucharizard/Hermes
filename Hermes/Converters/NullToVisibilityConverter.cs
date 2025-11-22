using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte valores null/no-null a Visibility
    /// null → Collapsed
    /// no-null → Visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
