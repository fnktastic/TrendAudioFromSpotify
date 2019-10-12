using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Service.Spotify
{
    public class SpotifyProvider
    {
        private static string _clientId = "b19f2661ded749a48c0dab9e0f2a0d56";

        private static string _secretId = "48473a52f4754d529009cc87fb12b2ee";

        private static string _redirectUri = "http://localhost:4002";

        private static string _serverUri = "http://localhost:4002";
        public static AuthorizationCodeAuth Authorization { get; set; } = new AuthorizationCodeAuth(
            _clientId, 
            _secretId, 
            _redirectUri, 
            _serverUri, 
            Scope.PlaylistReadPrivate | 
            Scope.PlaylistReadCollaborative |
            Scope.AppRemoteControl |
            Scope.UserLibraryRead);
        public static void Auth()
        {
            Authorization.Start();
            Authorization.OpenBrowser();
        }
    }
}
