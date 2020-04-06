using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Messenger.Default.Register<TogglePlaylistPublicMessage>(this, TogglePlaylistPublicMessageRecieved);
        }

        #endregion

        #region filters
        private bool FilteredPlaylistCollection_Filter(object obj)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
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
        private async void TogglePlaylistPublicMessageRecieved(TogglePlaylistPublicMessage obj)
        {
            try
            {
                if (obj.SeriesKey != Guid.Empty)
                {
                    var series = _playlists.Where(x => x.SeriesKey == obj.SeriesKey).ToList();

                    foreach (var volume in series)
                    {
                        if (volume.IsPublic != obj.IsPublic)
                            volume.IsPublic = obj.IsPublic;

                        await _spotifyServices.ChangePlaylistVisibility(volume.SpotifyId, volume.IsPublic).ContinueWith(async i =>
                        {
                            if (i.Status == TaskStatus.RanToCompletion)
                                await _playlistService.ChangeVisibility(volume, volume.IsPublic);
                            else
                            {
                                volume.IsPublic = !obj.IsPublic;
                            }
                        });
                    }
                }
                else
                {
                    var playlist = _playlists.FirstOrDefault(x => x.Id == obj.Id);

                    if (playlist != null)
                    {
                        playlist.IsPublic = obj.IsPublic;

                        await _spotifyServices.ChangePlaylistVisibility(playlist.SpotifyId, playlist.IsPublic).ContinueWith(async i =>
                        {
                            if (i.Status == TaskStatus.RanToCompletion)
                                await _playlistService.ChangeVisibility(playlist, playlist.IsPublic);
                            else
                            {
                                playlist.IsPublic = !obj.IsPublic;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void PlaylistBuiltMessageRecieved(PlaylistBuiltMessage message)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task FetchData()
        {
            try
            {
                _logger.Info("Fetching Playlists Data...");

                var playlists = await _dataService.GetAllPlaylistsAsync(true);

                Playlists = new PlaylistCollection(playlists);

                FilteredPlaylistCollection = GetAudiosCollectionView(_playlists);

                FilteredPlaylistCollection.Filter += FilteredPlaylistCollection_Filter;

                FilteredPlaylistCollection.CustomSort = new PlaylistsSorter();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public ListCollectionView GetAudiosCollectionView(IEnumerable<Playlist> playlists)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(playlists);
        }

        private void GetPlaylists(Playlist playlist)
        {
            try
            {
                SelectedPlaylist = playlist;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion

        #region commands
        private RelayCommand<Playlist> _syncPlaylistCommand;
        public RelayCommand<Playlist> SyncPlaylistCommand => _syncPlaylistCommand ?? (_syncPlaylistCommand = new RelayCommand<Playlist>(SyncPlaylist));
        private async void SyncPlaylist(Playlist playlist)
        {
            try
            {
                playlist.ProcessingInProgress = true;

                var syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

                playlist.SpotifyId = syncedPalylist.Id;
                playlist.Href = syncedPalylist.Href;
                playlist.Uri = syncedPalylist.Uri;
                playlist.IsPublic = syncedPalylist.Public;

                playlist.Owner = syncedPalylist.Owner.DisplayName;
                playlist.OwnerProfileUrl = syncedPalylist.Owner.Href;
                playlist.Cover = syncedPalylist.Images != null && syncedPalylist.Images.Count > 0 ? syncedPalylist.Images.First().Url : "null";

                await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, playlist.SpotifyId, playlist.Href, playlist.Uri,
                                                                    playlist.Owner, playlist.OwnerProfileUrl, playlist.Cover);
                await Task.Delay(TimeSpan.FromSeconds(2));

                playlist.ProcessingInProgress = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _selectPlaylistCommand;
        public RelayCommand<Playlist> SelectPlaylistCommand => _selectPlaylistCommand ?? (_selectPlaylistCommand = new RelayCommand<Playlist>(SelectPlaylist));
        private void SelectPlaylist(Playlist playlist)
        {
            try
            {
                SelectedPlaylist = playlist;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _deletePlaylistCommand;
        public RelayCommand<Playlist> DeletePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand<Playlist>(DeletePlaylist));
        private async void DeletePlaylist(Playlist playlist)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _openPlaylistInBrowserCommand;
        public RelayCommand<Playlist> OpenPlaylistInBrowserCommand => _openPlaylistInBrowserCommand ?? (_openPlaylistInBrowserCommand = new RelayCommand<Playlist>(OpenPlaylistInBrowser));
        private void OpenPlaylistInBrowser(Playlist playlist)
        {
            try
            {
                if (playlist.IsExported)
                    System.Diagnostics.Process.Start(playlist.PublicUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _togglePlaylistPublicCommand;
        public RelayCommand<Playlist> TogglePlaylistPublicCommand => _togglePlaylistPublicCommand ?? (_togglePlaylistPublicCommand = new RelayCommand<Playlist>(TogglePlaylistPublic));
        private void TogglePlaylistPublic(Playlist playlist)
        {
            try
            {
                Messenger.Default.Send<TogglePlaylistPublicMessage>(new TogglePlaylistPublicMessage(playlist.SeriesKey, playlist.Id, playlist.IsPublic));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion
    }
}
