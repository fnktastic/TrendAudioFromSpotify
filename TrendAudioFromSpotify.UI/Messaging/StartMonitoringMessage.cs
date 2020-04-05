using System;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class StartMonitoringMessage
    {
        public Guid MonitoringItemId { get; set; }

        public StartMonitoringMessage(string monitoringId)
        {
            MonitoringItemId = Guid.Parse(monitoringId);
        }
    }
}
