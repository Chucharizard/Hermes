using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte la prioridad de una tarea a un color de borde
    /// </summary>
    public class PrioridadToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Gray);

            string prioridad = value.ToString()?.ToLower() ?? "";

            return prioridad switch
            {
                "alta" or "urgente" => new SolidColorBrush(Color.FromRgb(231, 76, 60)), // #E74C3C - Rojo
                "media" or "normal" => new SolidColorBrush(Color.FromRgb(243, 156, 18)), // #F39C12 - Naranja
                "baja" => new SolidColorBrush(Color.FromRgb(39, 174, 96)),  // #27AE60 - Verde
                _ => new SolidColorBrush(Color.FromRgb(149, 165, 166))      // #95A5A6 - Gris
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
