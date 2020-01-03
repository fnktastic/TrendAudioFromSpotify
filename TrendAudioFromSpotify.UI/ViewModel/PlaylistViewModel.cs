using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using log4net.Core;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Messaging;
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
        #endregion

        #region constructor
        public PlaylistViewModel(IDataService dataService, IPlaylistService playlistService, ISpotifyServices spotifyServices)
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _dataService = dataService;
            _playlistService = playlistService;
            _spotifyServices = spotifyServices;

            FetchData().ConfigureAwait(true);

            Messenger.Default.Register<PlaylistBuiltMessage>(this, PlaylistBuiltMessageRecieved);
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

            if (playlist.DisplayName.ToUpper().Contains(_playlistSearchText.ToUpper()))
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

        private async Task<string> RemoveFromSpotifyConfirmation(string header, string message)
        {
            try
            {
                return await _dialogCoordinator.ShowInputAsync(this, header, message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in MonitoringViewModel.ShowMessage", ex);

                return string.Empty;
            }
        }
        #endregion

        #region methods
        private void PlaylistBuiltMessageRecieved(PlaylistBuiltMessage message)
        {
            var playlistsToRemove = Playlists.Where(x => x.Name == message.MonitoringItem.TargetPlaylistName).ToList();

            foreach (var playlistToRemove in playlistsToRemove)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var targetPlaylist = Playlists.FirstOrDefault(x => x.Id == playlistToRemove.Id);

                    if (targetPlaylist != null)
                    {
                        Playlists.Remove(targetPlaylist);
                    }
                });
            }

            var playlists = message.Playlists;

            foreach (var playlist in playlists)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Playlists.Add(playlist);
                });
            }

            if (message.MonitoringItem.AutoRecreatePlaylisOnSpotify || playlistsToRemove.Any(x => x.IsExported))
            {
                var targetPlaylists = Playlists.Where(x => x.Name == message.MonitoringItem.TargetPlaylistName);

                foreach (var playlist in targetPlaylists)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SyncPlaylistCommand.Execute(playlist);
                    });
                }
            }
        }

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

        private void GetPlaylists(Playlist playlist)
        {
            SelectedPlaylist = playlist;
        }
        #endregion

        #region commands
        private RelayCommand<Playlist> _syncPlaylistCommand;
        public RelayCommand<Playlist> SyncPlaylistCommand => _syncPlaylistCommand ?? (_syncPlaylistCommand = new RelayCommand<Playlist>(SyncPlaylist));
        private async void SyncPlaylist(Playlist playlist)
        {
            playlist.ProcessingInProgress = true;

            var syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

            playlist.SpotifyId = syncedPalylist.Id;
            playlist.Href = syncedPalylist.Href;
            playlist.Uri = syncedPalylist.Uri;

            playlist.Owner = syncedPalylist.Owner.DisplayName;
            playlist.OwnerProfileUrl = syncedPalylist.Owner.Href;
            playlist.Cover = syncedPalylist.Images != null && syncedPalylist.Images.Count > 0 ? syncedPalylist.Images.First().Url : "null";

            await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, playlist.SpotifyId, playlist.Href, playlist.Uri,
                                                                playlist.Owner, playlist.OwnerProfileUrl, playlist.Cover);
            await Task.Delay(TimeSpan.FromSeconds(2));

            playlist.ProcessingInProgress = false;
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
            playlist.ProcessingInProgress = true;

            if (playlist.IsExported)
            {
                string confirmMessage = await RemoveFromSpotifyConfirmation("Confirmation", string.Format("Also remove {0} from Spotify? Type 'yes' to confirm.", playlist.DisplayName));

                if (string.Equals(confirmMessage, "yes", StringComparison.OrdinalIgnoreCase))
                {
                    await _spotifyServices.RemovePlaylistAsync(playlist.SpotifyId);
                }
            }

            _playlists.Remove(playlist);

            await _dataService.RemovePlaylistAsync(playlist);

            playlist.ProcessingInProgress = false;
        }
        #endregion
    }
}
