using System;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class StartDailyMonitoringMessage
    {
        public Guid MonitoringItemId { get; set; }
        public bool Daily = true;

        public StartDailyMonitoringMessage(string monitoringId)
        {
            MonitoringItemId = Guid.Parse(monitoringId);
        }
    }
}
