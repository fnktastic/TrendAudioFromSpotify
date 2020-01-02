using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Schedule : ViewModelBase
    {
        public Guid Id { get; set; }

        private bool _repeatOn;
        public bool RepeatOn
        {
            get { return _repeatOn; }
            set
            {
                if (_repeatOn == value) return;
                _repeatOn = value;
                RaisePropertyChanged(nameof(RepeatOn));
            }
        }

        private DateTime? _startDateTime;
        public DateTime? StartDateTime
        {
            get { return _startDateTime; }
            set
            {
                if (_startDateTime == value) return;
                _startDateTime = value;
                RaisePropertyChanged(nameof(StartDateTime));
            }
        }

        private int _repeatInterval;
        public int RepeatInterval
        {
            get { return _repeatInterval; }
            set
            {
                if (_repeatInterval == value) return;
                _repeatInterval = value;
                RaisePropertyChanged(nameof(RepeatInterval));
            }
        }

        private RepeatModeEnum _repeatMode;
        public RepeatModeEnum RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                if (_repeatMode == value) return;
                _repeatMode = value;
                RaisePropertyChanged(nameof(RepeatMode));
            }
        }

        private DayOfWeek? _dayOfWeek;
        public DayOfWeek? DayOfWeek
        {
            get { return _dayOfWeek; }
            set
            {
                if (_dayOfWeek == value) return;
                _dayOfWeek = value;
                RaisePropertyChanged(nameof(DayOfWeek));
            }
        }

        public Schedule()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            if (StartDateTime.HasValue == false || StartDateTime.Value == DateTime.MinValue)
                StartDateTime = DateTime.Now;
        }
    }
}
