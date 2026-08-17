using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModelDoctor.Views.Converters
{
    /// <summary>
    /// Converts a hex color string (e.g., "#10B981") to a WPF <see cref="SolidColorBrush"/>.
    /// </summary>
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexString && !string.IsNullOrWhiteSpace(hexString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hexString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Fallback to Gray if invalid
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
