using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.ViewModel;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Playlist : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        private SimplePlaylist _simplePlaylist { get; set; }

        public string Id => _simplePlaylist.Id;

        public string Name => _simplePlaylist.Name;

        public int Total => _simplePlaylist.Tracks.Total;

        public string Owner => _simplePlaylist.Owner.DisplayName;

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
            _simplePlaylist = simplePlaylist;
        }
    }
}
