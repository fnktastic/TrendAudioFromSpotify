using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Controls;
using TrendAudioFromSpotify.UI.Extensions;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class AddSongToPlaylistViewModel : ViewModelBase
    {
        #region fields
        private readonly ISpotifyServices _spotifyServices;
        private List<Audio> likedSongs;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private bool _isTracksLoading;
        public bool IsTracksLoading
        {
            get { return _isTracksLoading; }
            set
            {
                if (_isTracksLoading == value) return;
                _isTracksLoading = value;
                RaisePropertyChanged(nameof(IsTracksLoading));
            }
        }

        private bool _isPlaylistsLoading;
        public bool IsPlaylistsLoading
        {
            get { return _isPlaylistsLoading; }
            set
            {
                if (_isPlaylistsLoading == value) return;
                _isPlaylistsLoading = value;
                RaisePropertyChanged(nameof(IsPlaylistsLoading));
            }
        }

        public CollectionViewSource FilteredAudioCollection { get; set; }
        public CollectionViewSource FilteredPlaylistsCollection { get; set; }

        private string _audiosSearchText;
        public string AudiosSearchText
        {
            get { return _audiosSearchText; }
            set
            {
                if (value == _audiosSearchText) return;
                _audiosSearchText = value;

                if (FilteredAudioCollection.View != null)
                    FilteredAudioCollection.View.Refresh();

                RaisePropertyChanged(nameof(AudiosSearchText));
            }
        }

        private string _playlistsSearchText;
        public string PlaylistsSearchText
        {
            get { return _playlistsSearchText; }
            set
            {
                if (value == _playlistsSearchText) return;
                _playlistsSearchText = value;

                if (FilteredPlaylistsCollection.View != null)
                    FilteredPlaylistsCollection.View.Refresh();

                if (string.IsNullOrWhiteSpace(_playlistsSearchText) == false)
                    CheckForUrl(_playlistsSearchText);

                RaisePropertyChanged(nameof(PlaylistsSearchText));
            }
        }

        private PlaylistCollection _playlists;
        public PlaylistCollection Playlists
        {
            get { return _playlists; }
            set
            {
                if (value == _playlists) return;
                _playlists = value;
                RaisePropertyChanged(nameof(Playlists));

                if (_playlists != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredPlaylistsCollection.Source = _playlists;
                    });
            }
        }

        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if (value == _selectedPlaylist) return;
                _selectedPlaylist = value;
                RaisePropertyChanged(nameof(SelectedPlaylist));

                if (_selectedPlaylist != null)
                {
                    GetPlaylistsAudios();
                }
            }
        }

        private AudioCollection _savedTracks;
        public AudioCollection SavedTracks
        {
            get { return _savedTracks; }
            set
            {
                if (value == _savedTracks) return;
                _savedTracks = value;

                if (_savedTracks != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredAudioCollection.Source = _savedTracks;
                    });

                RaisePropertyChanged(nameof(SavedTracks));
            }
        }

        private Audio _selectedAudio;
        public Audio SelectedAudio
        {
            get { return _selectedAudio; }
            set
            {
                if (value == _selectedAudio) return;
                _selectedAudio = value;
                RaisePropertyChanged(nameof(SelectedAudio));
            }
        }

        #endregion

        public AddSongToPlaylistViewModel(ISpotifyServices spotifyServices)
        {
            _spotifyServices = spotifyServices;

            FilteredAudioCollection = new CollectionViewSource();
            FilteredAudioCollection.Filter += FilteredAudioCollection_Filter;

            FilteredPlaylistsCollection = new CollectionViewSource();
            FilteredPlaylistsCollection.Filter += FilteredPlaylistsCollection_Filter;

            FetchData();
        }

        #region commands
        private RelayCommand _globalPlaylistsSearchCommand;
        public RelayCommand GlobalPlaylistsSearchCommand => _globalPlaylistsSearchCommand ?? (_globalPlaylistsSearchCommand = new RelayCommand(GlobalPlaylistsSearch));
        private async void GlobalPlaylistsSearch()
        {
            try
            {
                IsPlaylistsLoading = true;

                var playlists = await _spotifyServices.GlobalPlaylistsSearch(_playlistsSearchText);

                Playlists = new PlaylistCollection(playlists.Select(x => new Playlist(x)));

                PlaylistsSearchText = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                IsPlaylistsLoading = false;
            }
        }

        private RelayCommand _globalAudiosSearchCommand;
        public RelayCommand GlobalAudiosSearchCommand => _globalAudiosSearchCommand ?? (_globalAudiosSearchCommand = new RelayCommand(GlobalAudiosSearch));
        private async void GlobalAudiosSearch()
        {
            try
            {
                IsTracksLoading = true;

                var audios = await _spotifyServices.GlobalAudiosSearch(_audiosSearchText);

                SavedTracks = new AudioCollection(audios.Select(x => new Audio(x)));

                AudiosSearchText = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                IsTracksLoading = false;
            }
        }
        #endregion

        #region private methods
        private async void CheckForUrl(string s)
        {
            try
            {
                var uri = UrlValidator.Validate(s);

                if (uri != null)
                {
                    string playlistId = uri.Segments.Last();

                    var fullPlaylist = await _spotifyServices.GetPlaylistById(playlistId);

                    var playlist = new Playlist(fullPlaylist.ToSimple())
                    {
                        IsChecked = true
                    };

                    _playlists.Add(playlist);

                    PlaylistsSearchText = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void GetPlaylistsAudios()
        {
            try
            {
                IsTracksLoading = true;

                var audios = await _spotifyServices.GetPlaylistSongs(_selectedPlaylist.SpotifyId);
                SavedTracks = new AudioCollection(audios.Select(x => new Audio(x.Track)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                IsTracksLoading = false;
            }
        }

        void FilteredPlaylistsCollection_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_playlistsSearchText))
                {
                    e.Accepted = true;
                    return;
                }

                if (e.Item != null)
                {
                    if (e.Item is Playlist playlist)
                    {
                        if ((playlist.Name != null && playlist.Name.ToUpper().Contains(_playlistsSearchText.ToUpper())) ||
                            (playlist.Owner != null && playlist.Owner.ToUpper().Contains(_playlistsSearchText.ToUpper())))
                        {
                            e.Accepted = true;
                            return;
                        }
                    }
                }

                e.Accepted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                e.Accepted = false;
            }
        }

        void FilteredAudioCollection_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_audiosSearchText))
                {
                    e.Accepted = true;
                    return;
                }

                if (e.Item != null)
                {
                    if (e.Item is Audio audio)
                    {
                        if (audio.Title.ToUpper().Contains(_audiosSearchText.ToUpper()) ||
                            audio.Artist.ToUpper().Contains(_audiosSearchText.ToUpper()))
                        {
                            e.Accepted = true;
                            return;
                        }
                    }
                }

                e.Accepted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                e.Accepted = false;
            }
        }

        private async void FetchData()
        {
            try
            {
                IsTracksLoading = true;
                IsPlaylistsLoading = true;

                var playlists = (await _spotifyServices.GetAllPlaylists()).Select(x => new Playlist(x)).ToList();

                Playlists = new PlaylistCollection(playlists);

                likedSongs = (await _spotifyServices.GetSongs()).Select(x => new Audio(x.Track)).ToList();

                SavedTracks = new AudioCollection(likedSongs);

                var foreignUserPlaylists = await _spotifyServices.GetForeignUserPlaylists();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                IsTracksLoading = false;
                IsPlaylistsLoading = false;
            }
        }
        #endregion
    }
}
