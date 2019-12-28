using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if (value == _selectedPlaylist) return;
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
        public PlaylistViewModel(IDataService dataService, IPlaylistService playlistService)
        {
            _dataService = dataService;
            _playlistService = playlistService;

            FetchData().ConfigureAwait(true);
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
        private RelayCommand<Playlist> _syncPlaylistCommand;
        public RelayCommand<Playlist> SyncPlaylistCommand => _syncPlaylistCommand ?? (_syncPlaylistCommand = new RelayCommand<Playlist>(SyncPlaylist));
        private async void SyncPlaylist(Playlist playlist)
        {
            var syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

            await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, syncedPalylist.Id, syncedPalylist.Href);

            playlist.SpotifyId = syncedPalylist.Id;
            playlist.Href = syncedPalylist.Href;
        }
        #endregion
    }
}
