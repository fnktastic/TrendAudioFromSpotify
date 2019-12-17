using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private readonly IDataService _dataService;

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
                _selectedGroup = value;
                RaisePropertyChanged(nameof(SelectedGroup));
            }
        }

        public GroupManagingViewModel(IDataService dataService)
        {
            _dataService = dataService;

            FetchData();
        }

        private async void FetchData()
        {
            var groups = await _dataService.GetAllGroupsAsync();

            Groups = new GroupCollection(groups);
        }

        #region commands 
        private RelayCommand<Group> _selectGroupCommand;
        public RelayCommand<Group> SelectGroupCommand => _selectGroupCommand ?? (_selectGroupCommand = new RelayCommand<Group>(SelectGroup));
        private void SelectGroup(Group group)
        {
            SelectedGroup = group;
        }
        #endregion
    }
}
