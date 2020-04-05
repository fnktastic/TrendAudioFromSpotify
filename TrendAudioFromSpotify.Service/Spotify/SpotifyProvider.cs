using GalaSoft.MvvmLight.Messaging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Service.Spotify
{
    public interface ISpotifyProvider
    {
        void GetAccess(string clientId, string secretId, string redirectUri, string serverUri);
        Task GetAccess(string clientId, string secretId, string redirectUri, string serverUri, string refreshToken);
    }

    public class SpotifyProvider : ISpotifyProvider
    {
        private AuthorizationCodeAuth _authorization;

        private Token _token;

        public SpotifyProvider()
        {

        }

        public void GetAccess(string clientId, string secretId, string redirectUri, string serverUri)
        {
            _authorization = new AuthorizationCodeAuth(
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

            _authorization.AuthReceived += OnAuthResponse;

            Auth(_authorization);
        }

        public async Task GetAccess(string clientId, string secretId, string redirectUri, string serverUri, string refreshToken)
        {
            _authorization = new AuthorizationCodeAuth(
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

            _token = await _authorization.RefreshToken(refreshToken);

            SpotifyWebAPI api = new SpotifyWebAPI
            {
                AccessToken = _token.AccessToken,
                UseAutoRetry = true,
                TokenType = _token.TokenType
            };

            Messenger.Default.Send<SpotifyWebAPI>(api);
            Messenger.Default.Send<Token>(_token);
        }

        private void Auth(AuthorizationCodeAuth authorizationCodeAuth)
        {
            if (authorizationCodeAuth == null)
                throw new AccessViolationException();

            authorizationCodeAuth.Start();
            authorizationCodeAuth.OpenBrowser();
        }

        private async void OnAuthResponse(object sender, AuthorizationCode payload)
        {
            if (sender is AuthorizationCodeAuth authorization)
            {
                authorization.Stop();

                _token = await authorization.ExchangeCode(payload.Code);

                SpotifyWebAPI api = new SpotifyWebAPI
                {
                    AccessToken = _token.AccessToken,
                    UseAutoRetry = true,
                    TokenType = _token.TokenType
                };

                Messenger.Default.Send<SpotifyWebAPI>(api);
                Messenger.Default.Send<Token>(_token);
            }
        }
    }
}
