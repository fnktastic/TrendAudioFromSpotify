using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Playlist")]
    public class PlaylistDto
    {
        [Key]
        public string Id { get; set; }
        public string Href { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string OwnerProfileUrl { get; set; }
        public int Total { get; set; }
        public string Cover { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public PlaylistTypeEnum PlaylistType { get; set; }

        public virtual ICollection<PlaylistAudioDto> PlaylistAudios { get; set; }

        public virtual ICollection<GroupPlaylistDto> GroupPlaylists { get; set; }
    }

    public enum PlaylistTypeEnum
    {
        Standard,
        Fifo
    }
}
