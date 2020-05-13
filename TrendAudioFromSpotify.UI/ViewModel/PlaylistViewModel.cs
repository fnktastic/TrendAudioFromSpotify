using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Controls;
using TrendAudioFromSpotify.UI.Extensions;
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
            Messenger.Default.Register<RemoveSongFromPlaylistMessage>(this, RemoveSongFromPlaylistMessageRecieved);
            Messenger.Default.Register<ChangeSomgPositionMessage>(this, ChangeSomgPositionMessageRecieved);
            Messenger.Default.Register<SendSongToPlaylistMessage>(this, SendSongToPlaylistMessageRecieved);
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
        private async void SendSongToPlaylistMessageRecieved(SendSongToPlaylistMessage obj)
        {
            try
            {
                var selectedAudio = obj.Audio;

                var selectedPlaylist = this.SelectedPlaylist;

                int position = obj.NewPosition;

                //spotify
                if (selectedPlaylist.IsExported)
                    await _spotifyServices.SendToPlaylist(selectedPlaylist.SpotifyId, selectedAudio.Uri, position);

                //db
                await _playlistService.SendToPlaylist(obj.Audio, selectedPlaylist.Id, obj.NewPosition);

                //ui
                selectedPlaylist.Audios.Insert(obj.NewPosition, selectedAudio);

                //update total
                selectedPlaylist.Total = await _dataService.RecalcTotal(selectedPlaylist.Id);
                selectedPlaylist.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void ChangeSomgPositionMessageRecieved(ChangeSomgPositionMessage obj)
        {
            try
            {
                var selectedAudio = obj.Audio;

                var selectedPlaylist = this.SelectedPlaylist;

                //db
                var uris = await _playlistService.ChangeTrackPosition(selectedPlaylist.Id, selectedAudio.Id, obj.OldPosition, obj.NewPosition);

                if (uris == null) return;

                //spotify
                if (selectedPlaylist.IsExported)
                    await _spotifyServices.ReorderPlaylist(selectedPlaylist.SpotifyId, uris);

                //ui
                selectedPlaylist.Audios.RemoveAt(obj.OldPosition);

                selectedPlaylist.Audios.Insert(obj.NewPosition, selectedAudio);

                //update total
                selectedPlaylist.Total = await _dataService.RecalcTotal(selectedPlaylist.Id);
                selectedPlaylist.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void RemoveSongFromPlaylistMessageRecieved(RemoveSongFromPlaylistMessage obj)
        {
            try
            {
                var selectedAudio = obj.Audio;

                var selectedPlaylist = this.SelectedPlaylist;

                //spotify
                if (selectedPlaylist.IsExported)
                    await _spotifyServices.RemoveSongFromPlaylist(selectedPlaylist.SpotifyId, selectedAudio.Uri);

                //db
                await _playlistService.RemoveSongFromPlaylist(selectedPlaylist.Id, selectedAudio.Id);

                //ui
                selectedPlaylist.Audios.Remove(selectedAudio);

                //update total
                selectedPlaylist.Total = await _dataService.RecalcTotal(selectedPlaylist.Id);
                selectedPlaylist.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void TogglePlaylistPublicMessageRecieved(TogglePlaylistPublicMessage obj)
        {
            try
            {
                if (obj.SeriesKey != Guid.Empty)
                {
                    var series = _playlists.Where(x => x.SeriesKey == obj.SeriesKey).ToList();

                    foreach (var volume in series)
                    {
                        if (volume.IsExported == false) continue;

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

                    if (playlist != null && playlist.IsExported)
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

        private async Task DailyMonitoring(PlaylistBuiltMessage message)
        {
            var targetPlaylists = message.Playlists;

            if (message.MonitoringItem.AutoRecreatePlaylisOnSpotify == false)
                targetPlaylists = targetPlaylists.Where(x => x.IsExported).ToList();

            foreach (var playlist in targetPlaylists)
            {
                await Application.Current.Dispatcher.BeginInvoke((Action)(async () => await SendPlaylistToSpotify(message.MonitoringItem, playlist, true)));
            }
        }

        private async Task Monitoring(PlaylistBuiltMessage message)
        {
            var targetPlaylists = Playlists.Where(x => x.Name == message.MonitoringItem.TargetPlaylistName);

            if (message.MonitoringItem.AutoRecreatePlaylisOnSpotify == false)
                targetPlaylists = targetPlaylists.Where(x => x.IsExported).ToList();

            if (message.MonitoringItem.IsOverridePlaylists)
            {
                await _playlistService.ClearPlaylists(message.MonitoringItem);
            }

            foreach (var playlist in targetPlaylists)
            {
                if (message.MonitoringItem.IsOverridePlaylists == false)
                {
                    await Application.Current.Dispatcher.BeginInvoke((Action)(async () => await SendPlaylistToSpotify(message.MonitoringItem, playlist)));
                }

                if (message.MonitoringItem.IsOverridePlaylists)
                {
                    await Application.Current.Dispatcher.BeginInvoke((Action)(async () => await SendPlaylistToSpotify(message.MonitoringItem, playlist)));
                }
            }
        }

        private void ReplacePlaylistsWithUpdatetedOnUI(PlaylistBuiltMessage message)
        {
            var playlists = message.Playlists;

            foreach (var playlistToRemove in playlists)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var targetPlaylist = Playlists.FirstOrDefault(x => x.Name == message.MonitoringItem.TargetPlaylistName && x.SeriesNo == playlistToRemove.SeriesNo);

                    if (targetPlaylist != null)
                    {
                        Playlists.Remove(targetPlaylist);
                    }
                });
            }

            foreach (var playlist in playlists)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Playlists.Add(playlist);
                });
            }
        }

        private async void PlaylistBuiltMessageRecieved(PlaylistBuiltMessage message)
        {
            try
            {
                ReplacePlaylistsWithUpdatetedOnUI(message);

                //if (message.MonitoringItem.AutoRecreatePlaylisOnSpotify == false) return;

                if (message.MonitoringItem.IsDailyMonitoring)
                {
                    await DailyMonitoring(message);
                }

                if (message.MonitoringItem.IsDailyMonitoring == false)
                {
                    await Monitoring(message);
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

        private async Task SendPlaylistToSpotify(MonitoringItem monitoringItem, Playlist playlist, bool daily = false)
        {
            playlist.ProcessingInProgress = true;

            FullPlaylist syncedPalylist = null;

            if (daily)
                syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);
            else
            {
                if (monitoringItem.IsOverridePlaylists == false)
                    syncedPalylist = await _playlistService.RecreateOnSpotify(playlist);

                if (monitoringItem.IsOverridePlaylists)
                    syncedPalylist = await _playlistService.RecreateOnSpotifyWithClearing(playlist);
            }

            playlist.SpotifyId = syncedPalylist.Id;
            playlist.Href = syncedPalylist.Href;
            playlist.Uri = syncedPalylist.Uri;
            playlist.IsPublic = syncedPalylist.Public;

            playlist.Owner = syncedPalylist.Owner.DisplayName;
            playlist.OwnerProfileUrl = syncedPalylist.Owner.Href;
            playlist.Cover = syncedPalylist.Images != null && syncedPalylist.Images.Count > 0 ? syncedPalylist.Images.First().Url : "";

            await _dataService.AddSpotifyUriHrefToPlaylistAsync(playlist.Id, playlist.SpotifyId, playlist.Href, playlist.Uri,
                                                                playlist.Owner, playlist.OwnerProfileUrl, playlist.Cover);
            await Task.Delay(TimeSpan.FromSeconds(2));

            playlist.ProcessingInProgress = false;
        }
        #endregion

        #region commands
        private RelayCommand _sendToPlaylistCommand;
        public RelayCommand SendToPlaylistCommand => _sendToPlaylistCommand ?? (_sendToPlaylistCommand = new RelayCommand(SendToPlaylist));
        private void SendToPlaylist()
        {
            try
            {
                var addSongToPlaylistControlDialog = new ContentExplorerControlDialog();

                var addSongToPlaylistViewModel = new AddSongToPlaylistViewModel(_spotifyServices);

                addSongToPlaylistControlDialog.DataContext = addSongToPlaylistViewModel;

                addSongToPlaylistControlDialog.Show();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _syncWithExportedPlaylistCommand;
        public RelayCommand<Playlist> SyncWithExportedPlaylistCommand => _syncWithExportedPlaylistCommand ?? (_syncWithExportedPlaylistCommand = new RelayCommand<Playlist>(SyncWithExportedPlaylist));
        private async void SyncWithExportedPlaylist(Playlist playlist)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playlist.SpotifyId)) return;

                playlist.ProcessingInProgress = true;

                var spotifyPlaylist = new Playlist((await _spotifyServices.GetPlaylistById(playlist.SpotifyId)).ToSimple());

                if (spotifyPlaylist != null)
                {
                    playlist.SpotifyId = spotifyPlaylist.SpotifyId;
                    playlist.Total = spotifyPlaylist.Total;
                    playlist.Owner = spotifyPlaylist.Owner;
                    playlist.OwnerProfileUrl = spotifyPlaylist.OwnerProfileUrl;
                    playlist.Href = spotifyPlaylist.Href;
                    playlist.Uri = spotifyPlaylist.Uri;
                    playlist.IsPublic = spotifyPlaylist.IsPublic;
                    playlist.Cover = spotifyPlaylist.Cover ?? "";
                    playlist.UpdatedAt = DateTime.UtcNow;
                }

                await _playlistService.UpdatePlaylist(playlist);

                var tracks = (await _spotifyServices.GetPlaylistSongs(spotifyPlaylist.SpotifyId)).Select(x => new Audio(x.Track)).ToList();

                playlist.Audios = new AudioCollection(tracks);

                await _playlistService.UpdatePlaylistTracks(playlist, tracks);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                playlist.ProcessingInProgress = false;
            }
        }

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
                playlist.Cover = syncedPalylist.Images != null && syncedPalylist.Images.Count > 0 ? syncedPalylist.Images.First().Url : "";

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
                    string confirmMessage = await RemoveFromSpotifyConfirmation("Confirmation", string.Format("{0} is on Spotify. Type 'yes' to confirm.", playlist.DisplayName));

                    if (string.Equals(confirmMessage, "yes", StringComparison.OrdinalIgnoreCase))
                    {
                        await _spotifyServices.RemovePlaylistAsync(playlist.SpotifyId);
                    }
                    else 
                        return;
                }

                _playlists.Remove(playlist);

                await _dataService.RemovePlaylistAsync(playlist);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                playlist.ProcessingInProgress = false;
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
