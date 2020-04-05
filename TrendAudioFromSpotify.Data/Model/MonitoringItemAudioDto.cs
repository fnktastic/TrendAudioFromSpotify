using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("MonitoringItemAudio")]
    public class MonitoringItemAudioDto
    {
        [Key]
        public Guid MonitoringItemId { get; set; }
        public MonitoringItemDto MonitoringItem { get; set; }

        [Key]
        public string AudioId { get; set; }
        public AudioDto Audio { get; set; }

        public int Hits { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
