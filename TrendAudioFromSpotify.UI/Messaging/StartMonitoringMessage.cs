using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
