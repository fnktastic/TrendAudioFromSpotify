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
        public static AuthorizationCodeAuth Authorization { get; set; } = null; 

        public static void InitProvider(string clientId, string secretId, string redirectUri, string serverUri)
        {
            Authorization = new AuthorizationCodeAuth(
            clientId,
            secretId,
            redirectUri,
            serverUri,
            Scope.PlaylistModifyPublic |
            Scope.PlaylistModifyPrivate |
            Scope.UserFollowRead |
            Scope.UserReadPrivate |
            Scope.UserModifyPlaybackState | 
            Scope.UserReadPlaybackState |
            Scope.UserReadRecentlyPlayed |
            Scope.Streaming |
            Scope.UserReadCurrentlyPlaying |
            Scope.PlaylistReadPrivate |
            Scope.PlaylistReadCollaborative |
            Scope.AppRemoteControl |
            Scope.UserLibraryRead);
        }

        public static void Auth()
        {
            if (Authorization == null)
                throw new AccessViolationException();

            Authorization.Start();
            Authorization.OpenBrowser();
        }
    }
}
