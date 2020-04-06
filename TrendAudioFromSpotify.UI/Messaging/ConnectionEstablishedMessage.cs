using SpotifyAPI.Web.Models;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class ConnectionEstablishedMessage
    {
        public PublicProfile PublicProfile { get; set; }
        public PrivateProfile PrivateProfile { get; set; }

        public ConnectionEstablishedMessage(PrivateProfile privateProfile, PublicProfile publicProfile)
        {
            PrivateProfile = privateProfile;
            PublicProfile = publicProfile;
        }
        public ConnectionEstablishedMessage()
        {
                
        }
    }
}
