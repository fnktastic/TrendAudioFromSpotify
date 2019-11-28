using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Group
    {
        private bool readyToProcessing = false;
        private bool processingFinished = false;
        private bool processingSpecificAudios = false;

        public string Name { get; set; }
        public string Top { get; set; }
        public string HitTreshold { get; set; }
        public ComparisonEnum Comparison { get; set; }
        public PlaylistTypeEnum PlaylistType { get; set; }

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
