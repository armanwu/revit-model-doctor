using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ModelDoctor.Core;

namespace ModelDoctor.Views.Converters
{
    /// <summary>
    /// Converts a <see cref="HealthStatus"/> enum value to a corresponding WPF <see cref="SolidColorBrush"/> for status badges.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HealthStatus status)
            {
                return status switch
                {
                    HealthStatus.Pass => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                    HealthStatus.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12")),
                    HealthStatus.Fail => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
