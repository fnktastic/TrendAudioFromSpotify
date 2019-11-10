using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Audio")]
    public class AudioDto
    {
        public string Id { get; set; }
        public string Href { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<PlaylistAudioDto> PlaylistAudios { get; set; }
    }
}
