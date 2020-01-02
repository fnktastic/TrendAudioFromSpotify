using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("MonitoringItem")]
    public class MonitoringItemDto
    {
        [Key]
        public Guid Id { get; set; }

        public string MaxSize { get; set; }
        public string HitTreshold { get; set; }

        public ComparisonEnum Comparison { get; set; }
        public PlaylistTypeEnum PlaylistType { get; set; }

        public string TargetPlaylistName { get; set; }
        public bool AutoRecreatePlaylisOnSpotify { get; set; }
        public bool IsOverrideTrends { get; set; }
        public bool IsSeries { get; set; }

        [Key]
        public Guid GroupId { get; set; }
        public GroupDto Group { get; set; }

        [Key]
        public Guid ScheduleId { get; set; }
        public ScheduleDto Schedule { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public virtual ICollection<MonitoringItemAudioDto> Trends { get; set; }
    }

    public enum ComparisonEnum
    {
        Equals,
        More,
        Less
    }
}
