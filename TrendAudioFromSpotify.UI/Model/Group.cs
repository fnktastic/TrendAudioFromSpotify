using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Group : ViewModelBase
    {
        private bool readyToProcessing = false;
        private bool processingFinished = false;
        private bool processingSpecificAudios = false;

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

        private string _top;
        public string Top
        {
            get { return _top; }
            set
            {
                if (value == _top) return;
                _top = value;
                RaisePropertyChanged(nameof(Top));
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

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TimeSpan RefreshPeriod { get; set; }


        public virtual AudioCollection Audios { get; set; }
        public virtual PlaylistCollection Playlists { get; set; }
        public virtual AudioCollection Trends { get; set; }

        public Group()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Group(Group group, AudioCollection audios, PlaylistCollection playlists)
        {
            this.Name = group.Name;
            this.Top = group.Top;
            this.HitTreshold = group.HitTreshold;
            this.Comparison = group.Comparison;
            this.PlaylistType = group.PlaylistType;

            Audios = new AudioCollection(audios);
            Playlists = new PlaylistCollection(playlists);

            if (audios.Count > 0)
                processingSpecificAudios = true;

            CreatedAt = DateTime.UtcNow;

            readyToProcessing = true;
        }

        public void Process()
        {
            if(readyToProcessing)
            {

            }
        }
    }
}
