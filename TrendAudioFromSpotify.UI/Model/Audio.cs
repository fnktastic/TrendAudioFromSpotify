﻿using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Audio : ViewModelBase
    {
        private FullTrack _track { get; set; }

        public string Id => _track.Id;

        public string Artist => string.Join(" ", _track.Artists.Select(x => x.Name));

        public string Title => _track.Name;

        public string Href => _track.Href;

        public string Uri => _track.Uri;

        public int Hits { get; set; }

        public bool IsFilled => _track != null ? true : false;

        public virtual PlaylistCollection Playlists { get; set; }

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

        private int _no;
        public int No
        {
            get { return _no; }
            set
            {
                _no = value;
                RaisePropertyChanged(nameof(No));
            }
        }

        public Audio(FullTrack track)
        {
            _track = track;
        }

        public Audio()
        {

        }
    }
}
