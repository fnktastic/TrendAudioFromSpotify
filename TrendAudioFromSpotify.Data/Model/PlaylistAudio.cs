using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    public class PlaylistAudio
    {
        public string PlaylistId { get; set; }
        public Playlist Playlist { get; set; }

        public string AudioId { get; set; }
        public Audio Audio { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
