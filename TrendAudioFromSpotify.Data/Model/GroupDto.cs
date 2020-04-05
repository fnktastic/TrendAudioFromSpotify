using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Group")]
    public class GroupDto
    {
        [Key]
        public Guid Id { get; set; } //group
        public string Name { get; set; } //group

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public virtual ICollection<GroupPlaylistDto> GroupPlaylists { get; set; }

        public virtual ICollection<MonitoringItemDto> MonitoringItems { get; set; }

        [NotMapped]
        public virtual ICollection<PlaylistDto> Playlists => GroupPlaylists.Select(x => x.Playlist).ToList();
    }
}
