using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;
using DbContext = TrendAudioFromSpotify.Data.DataAccess.Context;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MonitoringViewModel : ViewModelBase
    {
        #region fields
        private readonly DbContext _dbContext;
        #endregion

        #region properties
        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get { return _groups; }
            set
            {
                if (value == _groups) return;
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
                if (value == _selectedGroup) return;
                _selectedGroup = value;
                RaisePropertyChanged(nameof(SelectedGroup));
            }
        }
        #endregion

        public MonitoringViewModel(DbContext dbContext)
        {
            _dbContext = dbContext;

            Groups = new ObservableCollection<Group>();
        }
    }
}
