﻿using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Playlist : ViewModelBase
    {
        public SimplePlaylist SimplePlaylist { get; set; }

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

        public Playlist(SimplePlaylist simplePlaylist)
        {
            SimplePlaylist = simplePlaylist;
        }
    }
}
