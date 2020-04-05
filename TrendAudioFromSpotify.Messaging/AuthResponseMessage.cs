namespace TrendAudioFromSpotify.Messaging
{
    public class AuthResponseMessage
    {
        public string AccessToken { get; set; }

        public AuthResponseMessage(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
