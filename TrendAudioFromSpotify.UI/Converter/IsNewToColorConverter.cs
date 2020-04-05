using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TrendAudioFromSpotify.UI.Converter
{
    public class IsNewToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isNew)
            {
                if (isNew) return Brushes.SpringGreen;

                return Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
