using System;
using System.Windows;
using System.Windows.Data;

namespace SilkDialectLearning.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((parameter as string) != "Header")
            {
                if (value != null)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            if (value == null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
