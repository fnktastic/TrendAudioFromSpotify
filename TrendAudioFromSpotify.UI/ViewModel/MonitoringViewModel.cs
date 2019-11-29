using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MonitoringViewModel : ViewModelBase
    {
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
        #endregion

        public MonitoringViewModel()
        {
            Groups = new ObservableCollection<Group>();
        }
    }
}
