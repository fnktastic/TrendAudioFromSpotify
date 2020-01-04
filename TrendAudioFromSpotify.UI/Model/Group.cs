using GalaSoft.MvvmLight;
using System;
using TrendAudioFromSpotify.UI.Collections;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Group : ViewModelBase
    {
        #region properties
        public Guid Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        public DateTime CreatedAt { get; set; }

        private DateTime _updateAt;
        public DateTime UpdatedAt
        {
            get { return _updateAt; }
            set
            {
                if (value == _updateAt) return;
                _updateAt = value;
                RaisePropertyChanged(nameof(UpdatedAt));
            }
        }

        public virtual PlaylistCollection Playlists { get; set; }

        public virtual MonitoringItemCollection MonitoringItems { get; set; }

        public virtual MonitoringItem GroupSourceMonitoringItem { get; set; } = new MonitoringItem();
        #endregion
    }
}
