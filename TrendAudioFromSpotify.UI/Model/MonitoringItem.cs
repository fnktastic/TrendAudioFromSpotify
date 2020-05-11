using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Controls;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.ViewModel;

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

        private TrendsSortingEnum _trendsSorting;
        public TrendsSortingEnum TrendsSorting
        {
            get { return _trendsSorting; }
            set
            {
                if (value == _trendsSorting) return;
                _trendsSorting = value;
                RaisePropertyChanged(nameof(TrendsSorting));
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

        private bool _isOverridePlaylists;
        public bool IsOverridePlaylists
        {
            get { return _isOverridePlaylists; }
            set
            {
                if (value == _isOverridePlaylists) return;
                _isOverridePlaylists = value;
                RaisePropertyChanged(nameof(IsOverridePlaylists));
            }
        }

        private bool _isDailyTrends;
        public bool IsDailyTrends
        {
            get { return _isDailyTrends; }
            set
            {
                if (value == _isDailyTrends) return;
                _isDailyTrends = value;
                RaisePropertyChanged(nameof(IsDailyTrends));
            }
        }

        private bool _isRandomizeGroup;
        public bool IsRandomizeGroup
        {
            get { return _isRandomizeGroup; }
            set
            {
                if (value == _isRandomizeGroup) return;
                _isRandomizeGroup = value;
                RaisePropertyChanged(nameof(IsRandomizeGroup));
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

        public bool IsReady { get; set; } = false;

        public virtual Group Group { get; set; }

        private Schedule _schedule;
        public virtual Schedule Schedule
        {
            get { return _schedule; }
            set
            {
                if (_schedule == value) return;
                _schedule = value;
                RaisePropertyChanged(nameof(Schedule));
            }
        }

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

        private TimeSpan _nextFireDateTime;
        public TimeSpan NextFireDateTime
        {
            get { return _nextFireDateTime; }
            set
            {
                if (value == _nextFireDateTime) return;
                _nextFireDateTime = value;
                RaisePropertyChanged(nameof(NextFireDateTime));
            }
        }

        public MonitoringItem()
        {
            Schedule = new Schedule();
        }

        #region commands
        private RelayCommand _setScheduleCommand;
        public RelayCommand SetScheduleCommand => _setScheduleCommand ?? (_setScheduleCommand = new RelayCommand(SetSchedule));
        private async void SetSchedule()
        {
            var dialogCoordinator = DialogCoordinator.Instance;

            var schedulingDialog = new ScheduleControlDialog();

            var schedulingViewModelDialog = new SchedulingViewModelDialog(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, schedulingDialog);
                if (!instance.IsCanceled)
                {
                    Schedule = instance.Schedule;
                    Schedule.RepeatOn = true;
                }
                if (instance.IsCanceled)
                {
                    Schedule.RepeatOn = false;
                }
            });

            schedulingDialog.DataContext = schedulingViewModelDialog;

            await dialogCoordinator.ShowMetroDialogAsync(this, schedulingDialog);
        }
        #endregion
    }
}
