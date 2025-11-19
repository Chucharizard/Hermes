using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte el estado de una tarea a un color de badge
    /// </summary>
    public class EstadoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Gray);

            string estado = value.ToString()?.ToLower() ?? "";

            return estado switch
            {
                "pendiente" => new SolidColorBrush(Color.FromRgb(243, 156, 18)),    // #F39C12 - Naranja
                "en progreso" or "en proceso" => new SolidColorBrush(Color.FromRgb(52, 152, 219)), // #3498DB - Azul
                "completada" or "completado" => new SolidColorBrush(Color.FromRgb(39, 174, 96)),  // #27AE60 - Verde
                "observada" or "devuelta" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),    // #E74C3C - Rojo
                "archivada" => new SolidColorBrush(Color.FromRgb(149, 165, 166)),   // #95A5A6 - Gris
                _ => new SolidColorBrush(Color.FromRgb(127, 140, 141))              // #7F8C8D - Gris oscuro
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
