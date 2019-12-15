﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("MonitoringItemDto")]
    public class MonitoringItemDto
    {
        [Key]
        public Guid Id { get; set; }

        public Guid GroupId { get; set; }
        public GroupDto Group { get; set; }

        public virtual ICollection<MonitoringItemAudioDto> Trends { get; set; }
    }
}
