﻿using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Audio : ViewModelBase
    {
        public FullTrack Track { get; set; }

        public string Artist => string.Join(" ", Track.Artists.Select(x => x.Name));

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public Audio(FullTrack track)
        {
            Track = track;
        }
    }
}
