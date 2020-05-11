using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class ChangePlaylistPositionMessage : ChangeItemPosition
    {
        public Playlist Playlist { get; set; }

        public ChangePlaylistPositionMessage(Playlist audio, int oldPosition, int newPosition)
        {
            Playlist = audio;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public ChangePlaylistPositionMessage()
        {

        }
    }
}
