﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using log4net.Core;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;
using TrendAudioFromSpotify.UI.Sorter;

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
                if (value == _playlists) return;
                _playlists = value;
                RaisePropertyChanged(nameof(Playlists));
            }
        }

        private string _playlistSearchText;
        public string PlaylistSearchText
        {
            get { return _playlistSearchText; }
            set
            {
                if (value == _playlistSearchText) return;
                _playlistSearchText = value;

                if (FilteredPlaylistCollection != null)
                    FilteredPlaylistCollection.Refresh();

                RaisePropertyChanged(nameof(PlaylistSearchText));
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

        #region filters
        private bool FilteredPlaylistCollection_Filter(object obj)
        {
            var playlist = obj as Playlist;

            if (string.IsNullOrWhiteSpace(_playlistSearchText))
            {
                return true;
            }

            if (playlist.Name.ToUpper().Contains(_playlistSearchText.ToUpper()))
            {
                return true;
            }

            return false;
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

        private ListCollectionView _filteredPlaylistCollection;
        public ListCollectionView FilteredPlaylistCollection
        {
            get => _filteredPlaylistCollection;
            set
            {
                if (Equals(_filteredPlaylistCollection, value)) return;
                _filteredPlaylistCollection = value;
                RaisePropertyChanged(nameof(FilteredPlaylistCollection));
            }
        }
        #endregion

        #region methods
        private async Task FetchData()
        {
            _logger.Info("Fetching Playlists Data...");

            var playlists = await _dataService.GetAllPlaylistsAsync(true);

            Playlists = new PlaylistCollection(playlists);

            FilteredPlaylistCollection = GetAudiosCollectionView(_playlists);

            FilteredPlaylistCollection.Filter += FilteredPlaylistCollection_Filter;

            FilteredPlaylistCollection.CustomSort = new PlaylistsSorter();
        }

        public ListCollectionView GetAudiosCollectionView(IEnumerable<Playlist> playlists)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(playlists);
        }
        #endregion

        #region commands
        private RelayCommand<Playlist> _syncPlaylistCommand;
        public RelayCommand<Playlist> SyncPlaylistCommand => _syncPlaylistCommand ?? (_syncPlaylistCommand = new RelayCommand<Playlist>(SyncPlaylist));
        private async void SyncPlaylist(Playlist playlist)
        {
            var syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

            playlist.SpotifyId = syncedPalylist.Id;
            playlist.Href = syncedPalylist.Href;
            playlist.Uri = syncedPalylist.Uri;

            playlist.Owner = syncedPalylist.Owner.DisplayName;
            playlist.OwnerProfileUrl = syncedPalylist.Owner.Href;
            playlist.Cover = syncedPalylist.Images != null && syncedPalylist.Images.Count > 0 ? syncedPalylist.Images.First().Url : "null";

            await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, playlist.SpotifyId, playlist.Href, playlist.Uri,
                                                                playlist.Owner, playlist.OwnerProfileUrl, playlist.Cover);
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
