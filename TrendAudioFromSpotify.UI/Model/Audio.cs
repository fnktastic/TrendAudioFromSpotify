using GalaSoft.MvvmLight;
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
        private FullTrack _track { get; set; }

        public string Id => _track.Id;

        public string Artist => string.Join(" ", _track.Artists.Select(x => x.Name));

        public string Title => _track.Name;

        public string Href => _track.Href;

        public string Uri => _track.Uri;

        public int Hits { get; set; }

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
            _track = track;
        }
    }
}
