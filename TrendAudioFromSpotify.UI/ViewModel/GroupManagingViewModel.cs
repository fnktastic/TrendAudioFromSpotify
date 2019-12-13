using AutoMapper;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.Repository;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class GroupManagingViewModel : ViewModelBase
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IMapper _mapper;

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

        public GroupManagingViewModel(IGroupRepository groupPlaylist, IMapper mapper)
        {
            _groupRepository = groupPlaylist;
            _mapper = mapper;

            FetchData();
        }

        private async void FetchData()
        {
            var groups = await _groupRepository.GetAllAsync();

            Groups = new GroupCollection(_mapper.Map<List<Group>>(groups));
        }
    }
}
