using System;
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
        public Guid Id { get; set; } //group
        public string Name { get; set; } //group
     
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<GroupPlaylistDto> GroupPlaylists { get; set; }

        public virtual ICollection<MonitoringItemDto> MonitoringItems { get; set; }
    }
}
