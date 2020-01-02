using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class PlaylistBuiltMessage
    {
        public MonitoringItem MonitoringItem { get; set; }

        public PlaylistBuiltMessage(MonitoringItem monitoringItem)
        {
            MonitoringItem = monitoringItem;
        }
    }
}
