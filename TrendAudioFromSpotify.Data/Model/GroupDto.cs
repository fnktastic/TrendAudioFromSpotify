﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Group")]
    public class GroupDto
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Top { get; set; }
        public string HitTreshold { get; set; }

        public ComparisonEnum Comparison { get; set; }
        public PlaylistTypeEnum PlaylistType { get; set; }

        public long TimeSpanTicks { get; set; }
        [NotMapped]
        public TimeSpan RefreshPeriod
        {
            get { return TimeSpan.FromTicks(TimeSpanTicks); }
            set { TimeSpanTicks = value.Ticks; }
        }

        public string TargetPlaylistName { get; set; }
        public bool AutoRecreatePlaylisOnSpotify { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Collection<AudioDto> Audios { get; set; }
        public virtual Collection<PlaylistDto> Playlists { get; set; }

        public virtual ICollection<GroupPlaylistDto> GroupPlaylists { get; set; }
    }

    public enum ComparisonEnum
    {
        Equals,
        More,
        Less
    }
}