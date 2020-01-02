using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Messaging;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Audio : ViewModelBase
    {
        private FullTrack _track { get; set; }

        public string Id { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string Href { get; set; }

        public string Uri { get; set; }

        public int Hits { get; set; }

        public long Duration { get; set; }

        public int Popularity { get; set; }

        public string Album { get; set; }

        public string Cover { get; set; } = "null";

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

        private bool _isNew;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew == value) return;
                _isNew = value;
                RaisePropertyChanged(nameof(IsNew));
            }
        }

        public Audio(FullTrack track)
        {
            _track = track;

            if (_track == null) return;

            Id = _track.Id;
            Href = _track.Href;
            Artist = string.Join(" ", _track.Artists.Select(x => x.Name));
            Title = _track.Name;
            Uri = _track.Uri;
            Popularity = _track.Popularity;
            Duration = _track.DurationMs;
            Album = _track.Album.Name;

            IsNew = false;

            if (_track.Album != null && _track.Album.Images != null && _track.Album.Images.Count > 0)
            {
                Cover = _track.Album.Images.First().Url;
            }
        }

        public Audio()
        {

        }

        private RelayCommand<Audio> _playSongCommand;
        public RelayCommand<Audio> PlaySongCommand => _playSongCommand ?? (_playSongCommand = new RelayCommand<Audio>(PlaySong));
        private void PlaySong(Audio audio)
        {
            Messenger.Default.Send<PlayAudioMessage>(new PlayAudioMessage(audio));
        }
    }
}
