using Humanizer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TrendAudioFromSpotify.UI.Converter
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is TimeSpan timeSpan)
            {
                if (TimeSpan.Zero == timeSpan) return "Next fire in <no data>";

                return string.Format("Next fire in {0}", timeSpan.Humanize(culture: CultureInfo.InvariantCulture));
            }

            return "Next fire in <no data>";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
