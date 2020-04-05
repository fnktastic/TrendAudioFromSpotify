using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("GroupPlaylist")]
    public class GroupPlaylistDto
    {
        [Key]
        public Guid GroupId { get; set; }
        public GroupDto Group { get; set; }

        [Key]
        public Guid PlaylistId { get; set; }
        public PlaylistDto Playlist { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
