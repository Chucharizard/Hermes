using System;
using System.Globalization;
using System.Windows.Data;

namespace Hermes.Converters
{
    public class PercentageWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return 0.0;

            // values[0] = percentage (0-100)
            // values[1] = container width

            if (values[0] is double percentage && values[1] is double containerWidth)
            {
                // Convert percentage (0-100) to actual width
                var width = (percentage / 100.0) * containerWidth;
                return Math.Max(0, Math.Min(width, containerWidth));
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
