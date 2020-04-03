using System;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class TogglePlaylistPublicMessage
    {
        public Guid SeriesKey { get; set; }
        public Guid Id { get; set; }
        public bool IsPublic { get; set; }

        public TogglePlaylistPublicMessage(Guid seriesKey, Guid id, bool isPublic)
        {
            SeriesKey = seriesKey;
            Id = id;
            IsPublic = isPublic;
        }
    }
}
