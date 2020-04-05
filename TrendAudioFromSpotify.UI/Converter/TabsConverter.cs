using System;
using System.Windows.Data;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Converter
{
    public class TabsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (TabsEnum)value;
        }
    }
}
