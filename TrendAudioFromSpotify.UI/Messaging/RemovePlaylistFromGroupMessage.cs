using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class RemovePlaylistFromGroupMessage
    {
        public Playlist Playlist { get; set; }

        public RemovePlaylistFromGroupMessage(Playlist playlist)
        {
            Playlist = playlist;
        }

        public RemovePlaylistFromGroupMessage()
        {
                
        }
    }
}
