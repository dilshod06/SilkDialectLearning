using System;
using System.Windows;
using System.Windows.Data;

namespace SilkDialectLearning.Converters
{
    public class PositionToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double xpos = (double)values[0];
            double ypos = (double)values[1];
            double dotHeight = (double)values[2];
            double dotWidth = (double)values[3];
            double width = (double)values[4];
            double height = (double)values[5];
            double left = (xpos * width / 100) - dotWidth / 2;
            double top = (ypos * height / 100) - dotHeight / 2;
            return new Thickness(left,top,0,0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
