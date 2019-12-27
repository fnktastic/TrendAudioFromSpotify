using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.ViewModel;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Playlist : ViewModelBase
    {
        private SimplePlaylist _simplePlaylist { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public int Total { get; set; }

        public string Owner { get; set; }

        public string OwnerProfileUrl { get; set; }

        public string Href { get; set; }

        public string Cover { get; set; }

        public bool SelfMade { get; set; } = false;

        public bool IsSeries { get; set; } = false;

        public Guid SeriesKey { get; set; }

        public int SeriesNo { get; set; }

        public virtual AudioCollection Audios { get; set; }

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

            Id = _simplePlaylist.Id;
            Name = _simplePlaylist.Name;
            Total = _simplePlaylist.Tracks.Total;
            Owner = _simplePlaylist.Owner.DisplayName;
            OwnerProfileUrl = _simplePlaylist.Owner.Href;
            Href = _simplePlaylist.Href;
            Cover = _simplePlaylist.Images != null && _simplePlaylist.Images.Count > 0 ? _simplePlaylist.Images.First().Url : "null";
        }

        public Playlist()
        {

        }
    }
}
