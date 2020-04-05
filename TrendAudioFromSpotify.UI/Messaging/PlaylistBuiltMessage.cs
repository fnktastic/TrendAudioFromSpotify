using System.Collections.Generic;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class PlaylistBuiltMessage
    {
        public MonitoringItem MonitoringItem { get; set; }

        public List<Playlist> Playlists { get; set; }

        public PlaylistBuiltMessage(MonitoringItem monitoringItem, List<Playlist> playlists)
        {
            MonitoringItem = monitoringItem;
            Playlists = playlists;
        }
    }
}
