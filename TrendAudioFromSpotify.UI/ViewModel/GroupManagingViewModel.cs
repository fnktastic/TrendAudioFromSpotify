using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;
using TrendAudioFromSpotify.UI.Sorter;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class GroupManagingViewModel : ViewModelBase
    {
        #region fields
        private readonly IDataService _dataService;
        private readonly IGroupService _groupService;
        public ISpotifyServices SpotifyServices = null;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private ListCollectionView _filteredGroupCollection;
        public ListCollectionView FilteredGroupCollection
        {
            get => _filteredGroupCollection;
            set
            {
                if (Equals(_filteredGroupCollection, value)) return;
                _filteredGroupCollection = value;
                RaisePropertyChanged(nameof(FilteredGroupCollection));
            }
        }

        private string _groupSearchText;
        public string GroupSearchText
        {
            get { return _groupSearchText; }
            set
            {
                if (value == _groupSearchText) return;
                _groupSearchText = value;

                if (FilteredGroupCollection != null)
                    FilteredGroupCollection.Refresh();

                RaisePropertyChanged(nameof(GroupSearchText));
            }
        }

        private static GroupCollection _groups = new GroupCollection();
        public GroupCollection Groups
        {
            get { return _groups; }
            set
            {
                if (_groups == value) return;
                _groups = value;
                RaisePropertyChanged(nameof(Groups));
            }
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (_selectedGroup == value) return;

                if (_selectedGroup != null)
                    _selectedGroup.IsExpanded = false;

                _selectedGroup = value;
                RaisePropertyChanged(nameof(SelectedGroup));
            }
        }
        #endregion

        public GroupManagingViewModel(IDataService dataService, IGroupService groupService)
        {
            _dataService = dataService;

            _groupService = groupService;

            FetchData().ConfigureAwait(true);

            Messenger.Default.Register<Group>(this, ReceiveSelectGroupMessage);
            Messenger.Default.Register<AddGroupMessage>(this, AddGroupMessage);
            Messenger.Default.Register<RemovePlaylistFromGroupMessage>(this, RecieveRemovePlaylistFromGroupMessage);
        }

        #region private methods
        public static Group GetFreshGroup(Group group) => _groups.FirstOrDefault(x => x.Id == group.Id);

        public ListCollectionView GetAudiosCollectionView(IEnumerable<Group> groups)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(groups);
        }

        private async Task FetchData()
        {
            _logger.Info("Fetching Groups Data...");

            var groups = await _dataService.GetAllGroupsAsync();

            Groups = new GroupCollection(groups);

            FilteredGroupCollection = GetAudiosCollectionView(_groups);

            FilteredGroupCollection.Filter += FilteredGroupCollection_Filter;

            FilteredGroupCollection.CustomSort = new GroupSorter();
        }
        #endregion

        #region filters
        private bool FilteredGroupCollection_Filter(object obj)
        {
            try
            {
                var group = obj as Group;

                if (string.IsNullOrWhiteSpace(_groupSearchText))
                {
                    return true;
                }

                if (group.Name.ToUpper().Contains(_groupSearchText.ToUpper()))
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

        #region commands 
        private RelayCommand<Group> _selectGroupCommand;
        public RelayCommand<Group> SelectGroupCommand => _selectGroupCommand ?? (_selectGroupCommand = new RelayCommand<Group>(SelectGroup));
        private void SelectGroup(Group group)
        {
            try
            {
                SelectedGroup = group;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Group> _deleteGroupCommand;
        public RelayCommand<Group> DeleteGroupCommand => _deleteGroupCommand ?? (_deleteGroupCommand = new RelayCommand<Group>(DeleteGroup));
        private async void DeleteGroup(Group group)
        {
            try
            {
                _groups.Remove(group);

                await _dataService.RemoveGroupAsync(group);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Group> _repeatGroupCommand;
        public RelayCommand<Group> RepeatGroupCommand => _repeatGroupCommand ?? (_repeatGroupCommand = new RelayCommand<Group>(RepeatGroup));
        private async void RepeatGroup(Group group)
        {
            try
            {
                await _groupService.MonitorGroupAsync(SpotifyServices, group);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion

        #region messages
        private void ReceiveSelectGroupMessage(Group group)
        {
            try
            {
                var targetGroup = Groups
                    .FirstOrDefault(x => x.Name == group.Name && x.CreatedAt == group.CreatedAt);

                if (targetGroup != null)
                    SelectedGroup = targetGroup;
                else
                    SelectedGroup = group;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void RecieveRemovePlaylistFromGroupMessage(RemovePlaylistFromGroupMessage obj)
        {
            try
            {
                var playlist = obj.Playlist;

                _selectedGroup.Playlists.Remove(playlist);

                await _dataService.RemovePlaylistFromGroupAsync(_selectedGroup, playlist);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void AddGroupMessage(AddGroupMessage addGroupMessage)
        {
            try
            {
                if (addGroupMessage != null && addGroupMessage.Group != null)
                    _groups.Add(addGroupMessage.Group);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion
    }
}
