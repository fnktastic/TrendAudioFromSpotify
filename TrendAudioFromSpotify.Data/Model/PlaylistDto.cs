using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Playlist")]
    public class PlaylistDto
    {
        [Key]
        public Guid Id { get; set; }

        public string SpotifyId { get; set; }
        public string Href { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string OwnerProfileUrl { get; set; }
        public int Total { get; set; }
        public string Cover { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public bool MadeByUser { get; set; }
        public bool IsSeries { get; set; } = false;
        public Guid SeriesKey { get; set; }
        public int SeriesNo { get; set; }
        public string Uri { get; set; }

        public PlaylistTypeEnum PlaylistType { get; set; }

        public virtual ICollection<PlaylistAudioDto> PlaylistAudios { get; set; }

        [NotMapped]
        public virtual ICollection<AudioDto> Audios => PlaylistAudios?.OrderBy(x => x.Placement).Select(x => x.Audio).ToList();

        public virtual ICollection<GroupPlaylistDto> GroupPlaylists { get; set; }
    }

    public enum PlaylistTypeEnum
    {
        Standard,
        Fifo
    }
}
