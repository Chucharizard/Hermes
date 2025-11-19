using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Hermes.Converters
{
    /// <summary>
    /// Convierte nombres y apellidos a iniciales para el avatar
    /// Ejemplo: "Juan", "Pérez" -> "JP"
    /// </summary>
    public class NombreToInitialsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return "??";

            string nombres = values[0]?.ToString() ?? "";
            string apellidos = values[1]?.ToString() ?? "";

            // Tomar primera letra del nombre y apellido
            string inicial1 = !string.IsNullOrWhiteSpace(nombres) ? nombres.Substring(0, 1).ToUpper() : "?";
            string inicial2 = !string.IsNullOrWhiteSpace(apellidos) ? apellidos.Substring(0, 1).ToUpper() : "?";

            return inicial1 + inicial2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
