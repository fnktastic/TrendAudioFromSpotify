﻿using System;
using System.Globalization;
using System.Windows.Data;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Converter
{
    public class AudiosContextNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Playlist playlist)
                return playlist.Name;

            return "My Library";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
