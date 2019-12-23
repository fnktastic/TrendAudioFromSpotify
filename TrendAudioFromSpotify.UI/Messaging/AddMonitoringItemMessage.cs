using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class AddMonitoringItemMessage
    {
        public MonitoringItem MonitoringItem { get; set; }

        public AddMonitoringItemMessage(MonitoringItem monitoringItem)
        {
            MonitoringItem = monitoringItem;
        }
    }
}
