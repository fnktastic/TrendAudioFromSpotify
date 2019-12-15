﻿using GalaSoft.MvvmLight;
using System;
using TrendAudioFromSpotify.UI.Collections;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Group : ViewModelBase
    {
        #region properties
        public Guid Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual PlaylistCollection Playlists { get; set; }
        #endregion
    }
}
