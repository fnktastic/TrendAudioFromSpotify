﻿using AutoMapper;
using GalaSoft.MvvmLight;
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

        public GroupManagingViewModel(IDataService dataService)
        {
            _dataService = dataService;

            FetchData();
        }

        private async void FetchData()
        {
            var groups = await _dataService.GetAllGroups();

            Groups = new GroupCollection(groups);
        }
    }
}