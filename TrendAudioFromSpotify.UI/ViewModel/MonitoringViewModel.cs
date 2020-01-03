using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;
using TrendAudioFromSpotify.UI.Sorter;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MonitoringViewModel : ViewModelBase
    {
        #region fields
        private readonly ISpotifyServices _spotifyServices;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDataService _dataService;
        private readonly IMonitoringService _monitoringService;
        private readonly IPlaylistService _playlistService;
        private readonly ISchedulingService _schedulingService;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private ListCollectionView _filteredMonitoringItemCollection;
        public ListCollectionView FilteredMonitoringItemCollection
        {
            get => _filteredMonitoringItemCollection;
            set
            {
                if (Equals(_filteredMonitoringItemCollection, value)) return;
                _filteredMonitoringItemCollection = value;
                RaisePropertyChanged(nameof(FilteredMonitoringItemCollection));
            }
        }

        private string _monitoringItemSearchText;
        public string MonitoringItemSearchText
        {
            get { return _monitoringItemSearchText; }
            set
            {
                if (value == _monitoringItemSearchText) return;
                _monitoringItemSearchText = value;

                if (FilteredMonitoringItemCollection != null)
                    FilteredMonitoringItemCollection.Refresh();

                RaisePropertyChanged(nameof(MonitoringItemSearchText));
            }
        }



        private MonitoringItemCollection _monitoringItems;
        public MonitoringItemCollection MonitoringItems
        {
            get { return _monitoringItems; }
            set
            {
                if (value == _monitoringItems) return;
                _monitoringItems = value;
                RaisePropertyChanged(nameof(MonitoringItems));
            }
        }

        private MonitoringItem _selectedMonitoringItem;
        public MonitoringItem SelectedMonitoringItem
        {
            get { return _selectedMonitoringItem; }
            set
            {
                if (value == _selectedMonitoringItem) return;

                if (_selectedMonitoringItem != null)
                    _selectedMonitoringItem.IsExpanded = false;

                _selectedMonitoringItem = value;
                RaisePropertyChanged(nameof(SelectedMonitoringItem));

                if (_selectedMonitoringItem != null)
                    FetchTrends().ConfigureAwait(true);
            }
        }
        #endregion

        public MonitoringViewModel(ISchedulingService schedulingService, IDataService dataService, IMonitoringService monitoringService, ISpotifyServices spotifyServices, IPlaylistService playlistService)
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _dataService = dataService;
            _monitoringService = monitoringService;
            _spotifyServices = spotifyServices;
            _playlistService = playlistService;
            _schedulingService = schedulingService;

            FetchData().ConfigureAwait(true);

            Messenger.Default.Register<AddMonitoringItemMessage>(this, AddMonitoringItemMessage);
            Messenger.Default.Register<ConnectionEstablishedMessage>(this, StartScheduling);
        }

        #region private methods
        public ListCollectionView GetAudiosCollectionView(IEnumerable<MonitoringItem> monitoringItems)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(monitoringItems);
        }

        private void AddMonitoringItemMessage(AddMonitoringItemMessage addMonitoringItemMessage)
        {
            if (addMonitoringItemMessage != null && addMonitoringItemMessage.MonitoringItem != null)
                _monitoringItems.Add(addMonitoringItemMessage.MonitoringItem);
        }

        private async Task FetchTrends()
        {
            var audios = await _dataService.GetAllMonitoringItemAudioByMonitoringItemIdAsync(_selectedMonitoringItem.Id);

            SelectedMonitoringItem.Trends = new AudioCollection(audios);
        }

        private async Task FetchData()
        {
            _logger.Info("Fetching Monitoring Data...");

            var monitoringItems = await _dataService.GetAllMonitoringItemsAsync();

            MonitoringItems = new MonitoringItemCollection(monitoringItems);

            FilteredMonitoringItemCollection = GetAudiosCollectionView(_monitoringItems);

            FilteredMonitoringItemCollection.Filter += FilteredMonitoringItemCollection_Filter;

            FilteredMonitoringItemCollection.CustomSort = new MonitoringItemSorter();
        }

        private async void StartScheduling(ConnectionEstablishedMessage message)
        {
            foreach (var monitoringItem in MonitoringItems)
            {
                await _schedulingService.ScheduleMonitoringItem(monitoringItem);
            }
        }
        #endregion

        #region filters
        private bool FilteredMonitoringItemCollection_Filter(object obj)
        {
            var monitoringItem = obj as MonitoringItem;

            if (string.IsNullOrWhiteSpace(_monitoringItemSearchText))
            {
                return true;
            }

            if (monitoringItem.TargetPlaylistName.ToUpper().Contains(_monitoringItemSearchText.ToUpper()))
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
        #endregion

        #region commands
        private RelayCommand<MonitoringItem> _selectMonitoringItemCommand;
        public RelayCommand<MonitoringItem> SelectMonitoringItemCommand => _selectMonitoringItemCommand ?? (_selectMonitoringItemCommand = new RelayCommand<MonitoringItem>(SelectMonitoringItem));
        private void SelectMonitoringItem(MonitoringItem monitoringItem)
        {
            SelectedMonitoringItem = monitoringItem;
        }

        private RelayCommand<MonitoringItem> _processMonitoringItemCommand;
        public RelayCommand<MonitoringItem> ProcessMonitoringItemCommand => _processMonitoringItemCommand ?? (_processMonitoringItemCommand = new RelayCommand<MonitoringItem>(ProcessMonitoringItem));
        private void ProcessMonitoringItem(MonitoringItem monitoringItem)
        {
            var _monitoringItem = _monitoringService.Initiate(monitoringItem.Group, monitoringItem, new AudioCollection(), monitoringItem.Group.Playlists);
            _monitoringService.ProcessAsync(_monitoringItem);
        }

        private RelayCommand<MonitoringItem> _deleteMonitoringItemCommand;
        public RelayCommand<MonitoringItem> DeleteMonitoringItemCommand => _deleteMonitoringItemCommand ?? (_deleteMonitoringItemCommand = new RelayCommand<MonitoringItem>(DeleteMonitoringItem));
        private async void DeleteMonitoringItem(MonitoringItem monitoringItem)
        {
            _monitoringItems.Remove(monitoringItem);

            await _dataService.RemoveMonitoringItemAsync(monitoringItem);
        }

        private RelayCommand<MonitoringItem> _goToBaseGoupCommand;
        public RelayCommand<MonitoringItem> GoToBaseGoupCommand => _goToBaseGoupCommand ?? (_goToBaseGoupCommand = new RelayCommand<MonitoringItem>(GoToBaseGoup));
        private void GoToBaseGoup(MonitoringItem monitoringItem)
        {
            Messenger.Default.Send<Group>(monitoringItem.Group);

            Messenger.Default.Send<TabsEnum>(TabsEnum.Groups);
        }

        private RelayCommand<MonitoringItem> _buildPlaylistCommand;
        public RelayCommand<MonitoringItem> BuildPlaylistCommand => _buildPlaylistCommand ?? (_buildPlaylistCommand = new RelayCommand<MonitoringItem>(BuildPlaylist));
        private async void BuildPlaylist(MonitoringItem monitoringItem)
        {
            var playlists = await _playlistService.BuildPlaylistAsync(monitoringItem);

            Messenger.Default.Send<PlaylistBuiltMessage>(new PlaylistBuiltMessage(monitoringItem, playlists));
        }
        #endregion
    }
}
