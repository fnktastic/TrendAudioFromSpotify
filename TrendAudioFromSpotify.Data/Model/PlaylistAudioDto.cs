using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("PlaylistAudio")]
    public class PlaylistAudioDto
    {
        public string PlaylistId { get; set; }
        public PlaylistDto Playlist { get; set; }

        public string AudioId { get; set; }
        public AudioDto Audio { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
