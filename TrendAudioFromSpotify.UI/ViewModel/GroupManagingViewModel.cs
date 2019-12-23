using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
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
    public class GroupManagingViewModel : ViewModelBase
    {
        #region fields
        private readonly IDataService _dataService;
        private readonly IGroupService _groupService;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        private GroupCollection _groups;
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

        public GroupManagingViewModel(IDataService dataService, IGroupService groupService)
        {
            _dataService = dataService;

            _groupService = groupService;

            FetchData().ConfigureAwait(true);

            Messenger.Default.Register<Group>(this, ReceiveSelectGroupMessage);
        }

        private async Task FetchData()
        {
            _logger.Info("Fetching Groups Data...");

            var groups = await _dataService.GetAllGroupsAsync();

            Groups = new GroupCollection(groups);
        }
        #endregion

        #region commands 
        private RelayCommand<Group> _selectGroupCommand;
        public RelayCommand<Group> SelectGroupCommand => _selectGroupCommand ?? (_selectGroupCommand = new RelayCommand<Group>(SelectGroup));
        private void SelectGroup(Group group)
        {
            SelectedGroup = group;
        }

        private RelayCommand<Group> _deleteGroupCommand;
        public RelayCommand<Group> DeleteGroupCommand => _deleteGroupCommand ?? (_deleteGroupCommand = new RelayCommand<Group>(DeleteGroup));
        private async void DeleteGroup(Group group)
        {
            _groups.Remove(group);

            await _dataService.RemoveGroupAsync(group);
        }

        private RelayCommand<Group> _repeatGroupCommand;
        public RelayCommand<Group> RepeatGroupCommand => _repeatGroupCommand ?? (_repeatGroupCommand = new RelayCommand<Group>(RepeatGroup));
        private async void RepeatGroup(Group group)
        {
            await _groupService.MonitorGroupAsync(group);
        }
        #endregion

        #region messages
        private void ReceiveSelectGroupMessage(Group group)
        {
            var targetGroup = Groups
                .FirstOrDefault(x => x.Name == group.Name && x.CreatedAt == group.CreatedAt);

            if (targetGroup != null)
                SelectedGroup = targetGroup;
            else
                SelectedGroup = group;
        }
        #endregion
    }
}
