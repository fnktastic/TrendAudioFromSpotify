using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class SendSongToPlaylistMessage
    {
        public Audio Audio { get; set; }
        public int NewPosition { get; set; }

        public SendSongToPlaylistMessage(Audio audio, int newPosition)
        {
            Audio = audio;
            NewPosition = newPosition;
        }

        public SendSongToPlaylistMessage()
        {

        }
    }
}
