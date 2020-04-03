using GalaSoft.MvvmLight;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.ViewModel;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Playlist : ViewModelBase
    {
        private SimplePlaylist _simplePlaylist { get; set; }

        public Guid Id { get; set; }

        public string SpotifyId { get; set; }

        public string Name { get; set; }

        public int Total { get; set; }

        public string OwnerProfileUrl { get; set; }

        public string Href { get; set; }

        public string Cover { get; set; }

        public bool MadeByUser { get; set; }

        public bool IsSeries { get; set; } = false;

        public Guid SeriesKey { get; set; }

        public int SeriesNo { get; set; }

        public PlaylistTypeEnum PlaylistType { get; set; }

        public DateTime CreatedAt { get; set; }

        private DateTime _updateAt;
        public DateTime UpdatedAt
        {
            get { return _updateAt; }
            set
            {
                if (value == _updateAt) return;
                _updateAt = value;
                RaisePropertyChanged(nameof(UpdatedAt));
            }
        }

        public virtual AudioCollection Audios { get; set; }

        public virtual string DisplayName
        {
            get
            {
                if (IsSeries)
                    return string.Format("{0} {1}", Name, SeriesNo);

                return Name;
            }
        }

        public virtual string PublicUrl => string.Format("https://open.spotify.com/playlist/{0}", SpotifyId);

        private bool _isPublic;
        public bool IsPublic
        {
            get { return _isPublic; }
            set
            {
                if (_isPublic == value) return;
                _isPublic = value;
                RaisePropertyChanged(nameof(IsPublic));
            }
        }

        public virtual bool IsExported
        {
            get
            {
                if (string.IsNullOrEmpty(Uri) == false)
                    return true;

                return false;
            }
        }

        private string _owner;
        public string Owner
        {
            get { return _owner; }
            set
            {
                if (value == _owner) return;
                _owner = value;
                RaisePropertyChanged(nameof(Owner));
            }
        }

        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set
            {
                if (value == _uri) return;
                _uri = value;
                RaisePropertyChanged(nameof(Uri));
                RaisePropertyChanged(nameof(IsExported));
            }
        }

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

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        private bool _processingInProgress;
        public bool ProcessingInProgress
        {
            get { return _processingInProgress; }
            set
            {
                if (value == _processingInProgress) return;
                _processingInProgress = value;
                RaisePropertyChanged(nameof(ProcessingInProgress));
            }
        }

        public Playlist(SimplePlaylist simplePlaylist)
        {
            _simplePlaylist = simplePlaylist;

            Id = Guid.NewGuid();
            SpotifyId = _simplePlaylist.Id;
            Name = _simplePlaylist.Name;
            Total = _simplePlaylist.Tracks.Total;
            Owner = _simplePlaylist.Owner.DisplayName;
            OwnerProfileUrl = _simplePlaylist.Owner.Href;
            Href = _simplePlaylist.Href;
            Uri = _simplePlaylist.Uri;
            IsPublic = _simplePlaylist.Public;
            Cover = _simplePlaylist.Images != null && _simplePlaylist.Images.Count > 0 ? _simplePlaylist.Images.First().Url : "null";
        }

        public Playlist()
        {
            Id = Guid.NewGuid();
        }
    }
}
