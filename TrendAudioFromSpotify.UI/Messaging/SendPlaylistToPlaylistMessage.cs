using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class SendPlaylistToPlaylistMessage
    {
        public Playlist Playlist { get; set; }

        public int NewPosition { get; set; }

        public SendPlaylistToPlaylistMessage(Playlist playlist, int newPosition)
        {
            Playlist = playlist;
            NewPosition = newPosition;
        }

        public SendPlaylistToPlaylistMessage()
        {

        }
    }
}
