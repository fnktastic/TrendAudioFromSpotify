using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class PlayAudioMessage
    {
        public Audio Audio { get; set; }

        public PlayAudioMessage(Audio audio)
        {
            Audio = audio;
        }
    }
}
