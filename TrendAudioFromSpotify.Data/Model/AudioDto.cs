using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Audio")]
    public class AudioDto
    {
        [Key]
        public string Id { get; set; }
        public string Href { get; set; }
        public string Uri { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public int Popularity { get; set; }
        public long Duration { get; set; }
        public string Album { get; set; }
        public string Cover { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public virtual ICollection<PlaylistAudioDto> PlaylistAudios { get; set; }

        [NotMapped]
        public virtual ICollection<PlaylistDto> Playlists => PlaylistAudios.Select(x => x.Playlist).ToList();
    }
}
