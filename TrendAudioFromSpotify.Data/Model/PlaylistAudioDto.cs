﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("PlaylistAudio")]
    public class PlaylistAudioDto
    {
        [Key]
        public Guid PlaylistId { get; set; }
        public PlaylistDto Playlist { get; set; }

        [Key]
        public string AudioId { get; set; }
        public AudioDto Audio { get; set; }

        public int Placement { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
