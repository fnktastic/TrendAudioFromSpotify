using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrendAudioFromSpotify.Data.Model
{
    [Table("Schedule")]
    public class ScheduleDto
    {
        [Key]
        public Guid Id { get; set; }
        public bool RepeatOn { get; set; }
        public DateTime? StartDateTime { get; set; }
        public int RepeatInterval { get; set; }
        public RepeatModeEnum RepeatMode { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
    }

    public enum RepeatModeEnum
    {
        SpecificDay,
        Hourly,
        Daily,
        Weekly,
        Monthly
    }
}
