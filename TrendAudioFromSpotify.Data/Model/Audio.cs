using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    public class Audio
    {
        public string Id { get; set; }
        public string Href { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<PlaylistAudio> PlaylistAudios { get; set; }
    }
}
