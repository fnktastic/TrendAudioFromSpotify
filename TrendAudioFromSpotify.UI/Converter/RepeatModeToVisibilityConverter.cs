using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Converter
{
    public class RepeatModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string mode = (string)parameter;

            if (mode == "0")
            {
                if (value is RepeatModeEnum repeatMode)
                {
                    if (repeatMode == RepeatModeEnum.SpecificDay)
                        return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }

            if (mode == "1")
            {
                if (value is RepeatModeEnum repeatMode)
                {
                    if (repeatMode == RepeatModeEnum.SpecificDay)
                        return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
