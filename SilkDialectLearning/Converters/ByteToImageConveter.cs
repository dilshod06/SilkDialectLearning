using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SilkDialectLearning.Converters
{
    public class ByteToImageConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] array = (byte[])value;
            BitmapImage image = new BitmapImage();
            MemoryStream ms = new MemoryStream(array);
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            return image as ImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
