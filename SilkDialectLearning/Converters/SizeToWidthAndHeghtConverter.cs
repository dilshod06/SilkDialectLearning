using System;
using System.Windows.Data;

namespace SilkDialectLearning.Converters
{
    public class SizeToWidthConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double size = (double)values[0];
            double width = (double)values[1];
            double resultWidth = (size * width) / 100;
            return resultWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SizeToHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double size = (double)values[0];
            double height = (double)values[1];
            return (size * height) / 100;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
