using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using log4net.Core;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class PlaylistViewModel : ViewModelBase
    {
        #region fields
        private readonly IDataService _dataService;
        private readonly IPlaylistService _playlistService;
        private readonly ISpotifyServices _spotifyServices;
        private readonly IDialogCoordinator _dialogCoordinator;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if (_selectedPlaylist == value) return;

                if (_selectedPlaylist != null)
                    _selectedPlaylist.IsExpanded = false;

                _selectedPlaylist = value;
                RaisePropertyChanged(nameof(SelectedPlaylist));
            }
        }

        private PlaylistCollection _playlists;
        public PlaylistCollection Playlists
        {
            get { return _playlists; }
            set
            {
                if (value ==_playlists) return;
                _playlists = value;
                RaisePropertyChanged(nameof(Playlists));
            }
        }
        #endregion

        #region constructor
        public PlaylistViewModel(IDataService dataService, IPlaylistService playlistService, ISpotifyServices spotifyServices)
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _dataService = dataService;
            _playlistService = playlistService;
            _spotifyServices = spotifyServices;

            FetchData().ConfigureAwait(true);
        }
        #endregion

        #region dialogs
        private async Task ShowMessage(string header, string message)
        {
            try
            {
                await _dialogCoordinator.ShowMessageAsync(this, header, message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in MonitoringViewModel.ShowMessage", ex);
            }
        }
        #endregion

        #region methods
        private async Task FetchData()
        {
            _logger.Info("Fetching Playlists Data...");

            var playlists = await _dataService.GetAllPlaylistsAsync(false);

            Playlists = new PlaylistCollection(playlists);
        }
        #endregion

        #region commands
        private RelayCommand<Audio> _playSongCommand;
        public RelayCommand<Audio> PlaySongCommand => _playSongCommand ?? (_playSongCommand = new RelayCommand<Audio>(PlaySong));
        private async void PlaySong(Audio audio)
        {
            try
            {
                if (audio != null)
                {
                    var playback = await _spotifyServices.PlayTrack(audio.Uri);

                    if (playback.HasError())
                        await ShowMessage("Playback Error", string.Format("Error code: {0}\n{1}\n{2}", playback.Error.Status, playback.Error.Message, "Make sure Spotify Client is opened and playback is working."));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in MonitoringViewModel.PlaySong", ex);
            }
        }

        private RelayCommand<Playlist> _syncPlaylistCommand;
        public RelayCommand<Playlist> SyncPlaylistCommand => _syncPlaylistCommand ?? (_syncPlaylistCommand = new RelayCommand<Playlist>(SyncPlaylist));
        private async void SyncPlaylist(Playlist playlist)
        {
            var syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

            await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, syncedPalylist.Id, syncedPalylist.Href);

            playlist.SpotifyId = syncedPalylist.Id;
            playlist.Href = syncedPalylist.Href;
        }

        private RelayCommand<Playlist> _selectPlaylistCommand;
        public RelayCommand<Playlist> SelectPlaylistCommand => _selectPlaylistCommand ?? (_selectPlaylistCommand = new RelayCommand<Playlist>(SelectPlaylist));
        private void SelectPlaylist(Playlist playlist)
        {
            SelectedPlaylist = playlist;
        }

        private RelayCommand<Playlist> _deletePlaylistCommand;
        public RelayCommand<Playlist> DeletePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand<Playlist>(DeletePlaylist));
        private async void DeletePlaylist(Playlist playlist)
        {
            _playlists.Remove(playlist);

            await _dataService.RemovePlaylistAsync(playlist);
        }
        #endregion
    }
}
