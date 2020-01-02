using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Model
{
    public class MonitoringItem : ViewModelBase
    {
        public Guid Id { get; set; }

        private string _maxSize;
        public string MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (value == _maxSize) return;
                _maxSize = value;
                RaisePropertyChanged(nameof(MaxSize));
            }
        }

        private string _hitTreshold;
        public string HitTreshold
        {
            get { return _hitTreshold; }
            set
            {
                if (value == _hitTreshold) return;
                _hitTreshold = value;
                RaisePropertyChanged(nameof(HitTreshold));
            }
        }

        private ComparisonEnum _comparison;
        public ComparisonEnum Comparison
        {
            get { return _comparison; }
            set
            {
                if (value == _comparison) return;
                _comparison = value;
                RaisePropertyChanged(nameof(Comparison));
            }
        }

        private PlaylistTypeEnum _playlistType;
        public PlaylistTypeEnum PlaylistType
        {
            get { return _playlistType; }
            set
            {
                if (value == _playlistType) return;
                _playlistType = value;
                RaisePropertyChanged(nameof(PlaylistType));
            }
        }

        private bool _processingInProgress;
        public bool ProcessingInProgress
        {
            get { return _processingInProgress; }
            set
            {
                if (value == _processingInProgress) return;
                _processingInProgress = value;
                RaisePropertyChanged(nameof(ProcessingInProgress));
            }
        }

        private string _targetPlaylistName;
        public string TargetPlaylistName
        {
            get { return _targetPlaylistName; }
            set
            {
                if (value == _targetPlaylistName) return;
                _targetPlaylistName = value;
                RaisePropertyChanged(nameof(TargetPlaylistName));
            }
        }

        private bool _autoRecreatePlaylisOnSpotify;
        public bool AutoRecreatePlaylisOnSpotify
        {
            get { return _autoRecreatePlaylisOnSpotify; }
            set
            {
                if (value == _autoRecreatePlaylisOnSpotify) return;
                _autoRecreatePlaylisOnSpotify = value;
                RaisePropertyChanged(nameof(AutoRecreatePlaylisOnSpotify));
            }
        }

        private bool _isOverrideTrends;
        public bool IsOverrideTrends
        {
            get { return _isOverrideTrends; }
            set
            {
                if (value == _isOverrideTrends) return;
                _isOverrideTrends = value;
                RaisePropertyChanged(nameof(IsOverrideTrends));
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

        private bool _isSeries;
        public bool IsSeries
        {
            get { return _isSeries; }
            set
            {
                if (value == _isSeries) return;
                _isSeries = value;
                RaisePropertyChanged(nameof(IsSeries));
            }
        }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsReady { get; set; } = false;

        [IgnoreMap]
        public virtual AudioCollection SpecificAudios { get; set; }
        public virtual PlaylistCollection Playlists { get; set; }
        public virtual Group Group { get; set; }
        public virtual Schedule Schedule { get; set; }

        private AudioCollection _trends;
        [IgnoreMap]
        public AudioCollection Trends
        {
            get { return _trends; }
            set
            {
                if (value == _trends) return;
                _trends = value;
                RaisePropertyChanged(nameof(Trends));
            }
        }

        #region commands
        private RelayCommand _setSchesuleCommand;
        public RelayCommand SetSchesuleCommand => _setSchesuleCommand ?? (_setSchesuleCommand = new RelayCommand(SetSchesule));
        private void SetSchesule()
        {
            Schedule = new Schedule();
        }
        #endregion
    }
}
