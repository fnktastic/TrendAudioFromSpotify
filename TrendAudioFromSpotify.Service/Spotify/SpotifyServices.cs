using GalaSoft.MvvmLight.Messaging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Messaging;
using Enums = SpotifyAPI.Web.Enums;

namespace TrendAudioFromSpotify.Service.Spotify
{
    public interface ISpotifyServices
    {
        Task GetAccess(string accessToken = null, string refreshToken = null);
        void Init(string clientId, string secretId, string redirectUri, string serverUri);
        Task<IEnumerable<SimplePlaylist>> GetAllPlaylists();
        Task<IEnumerable<SavedTrack>> GetSongs();
        Task<IEnumerable<PlaylistTrack>> GetPlaylistSongs(string playlistId);
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(string username = "_annalasnier_");
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(IList<string> usernames);
        Task<FullPlaylist> RecreatePlaylist(string playlistUri, string playlistName, IEnumerable<string> ids);
        Task<PublicProfile> GetMyProfile();
        Task<ErrorResponse> PlayTrack(string trackUri);
        Task<IEnumerable<SimplePlaylist>> GlobalPlaylistsSearch(string query);
        Task RemovePlaylistAsync(string playlistId);
    }

    public class SpotifyServices : ISpotifyServices
    {
        private readonly ISpotifyProvider _spotifyProvider;

        private bool authDataEntered = false;
        private string _clientId;
        private string _secretId;
        private string _redirectUri;
        private string _serverUri;

        private Token _token;

        private SpotifyWebAPI _spotifyWebAPI;

        private PrivateProfile _privateProfile;

        public SpotifyServices(ISpotifyProvider spotifyProvider)
        {
            _spotifyProvider = spotifyProvider;
            Messenger.Default.Register<SpotifyWebAPI>(this, OnSpotifyAccessRecieved);
            Messenger.Default.Register<Token>(this, OnSpotifyTokenRecieved);

        }

        public void Init(string clientId, string secretId, string redirectUri, string serverUri)
        {
            _clientId = clientId;
            _secretId = secretId;
            _redirectUri = redirectUri;
            _serverUri = serverUri;

            authDataEntered = true;
        }

        public async Task GetAccess(string accessToken = null, string refreshToken = null)
        {
            //if (string.IsNullOrWhiteSpace(accessToken) == false)
            //{
            //    AuthByToken(accessToken);

            //    _privateProfile = await _spotifyWebAPI.GetPrivateProfileAsync();

            //    if (_privateProfile.HasError() == false)
            //    {
            //        Messenger.Default.Send<AuthResponseMessage>(new AuthResponseMessage(accessToken));

            //        return;
            //    }
            //}

            if (string.IsNullOrWhiteSpace(refreshToken) == false)
            {
                await _spotifyProvider.GetAccess(_clientId, _secretId, _redirectUri, _serverUri, refreshToken);

                _privateProfile = await _spotifyWebAPI.GetPrivateProfileAsync();

                if (_privateProfile.HasError() == false)
                    return;
            }

            if (authDataEntered)
                _spotifyProvider.GetAccess(_clientId, _secretId, _redirectUri, _serverUri);
        }

        private void OnSpotifyAccessRecieved(SpotifyWebAPI spotifyWebAPI)
        {
            _spotifyWebAPI = spotifyWebAPI;

            _privateProfile = _spotifyWebAPI.GetPrivateProfile();

            Messenger.Default.Send<AuthResponseMessage>(new AuthResponseMessage(_spotifyWebAPI.AccessToken));
        }

        private void OnSpotifyTokenRecieved(Token token)
        {
            _token = token;
        }

        private void AuthByToken(string accessToken)
        {
            _spotifyWebAPI = new SpotifyWebAPI
            {
                AccessToken = accessToken,
                UseAutoRetry = true,
                TokenType = "Bearer"
            };
        }

        public async Task<IEnumerable<SimplePlaylist>> GetAllPlaylists()
        {
            int counter = 0;

            int limit = 20;

            var playlists = await _spotifyWebAPI.GetUserPlaylistsAsync(_privateProfile.Id, limit: 1, offset: 0);

            if (playlists.Error != null)
            {

            }

            int total = playlists.Total;

            var usersPlaylists = new List<SimplePlaylist>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(_privateProfile.Id, limit: limit, offset: counter))?.Items;

                counter += items.Count;

                usersPlaylists.AddRange(items);
            }

            return usersPlaylists;
        }

        public async Task<IEnumerable<SavedTrack>> GetSongs()
        {
            int counter = 0;
            int limit = 50;

            var tracks = await _spotifyWebAPI.GetSavedTracksAsync(limit: 1, offset: 0);

            if (tracks.Error != null)
            {

            }

            int total = tracks.Total;

            var songs = new List<SavedTrack>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetSavedTracksAsync(limit, counter))?.Items;

                counter += items.Count;

                songs.AddRange(items);
            }

            return songs;
        }

        [Obsolete]
        public async Task<IEnumerable<PlaylistTrack>> GetPlaylistSongs(string playlistId)
        {
            int counter = 0;
            int limit = 100;

            var tracks = await _spotifyWebAPI.GetPlaylistTracksAsync(playlistId: playlistId, limit: 1, offset: 0);

            if (tracks.Error != null)
            {

            }

            int total = tracks.Total;

            var playlistsSongs = new List<PlaylistTrack>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetPlaylistTracksAsync(playlistId: playlistId, limit: limit, offset: counter))?.Items;

                counter += items.Count; ;

                playlistsSongs.AddRange(items);
            }

            return playlistsSongs;
        }

        public async Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(string username = "_annalasnier_")
        {
            int counter = 0;
            int limit = 100;

            var playlists = await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: 1, offset: 0);

            if (playlists.Error != null)
            {

            }

            int total = playlists.Total;

            var usersPlaylists = new List<SimplePlaylist>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: limit, offset: counter))?.Items;

                counter += items.Count;

                usersPlaylists.AddRange(items);
            }

            return usersPlaylists;
        }

        public async Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(IList<string> usernames)
        {
            var usersPlaylists = new List<SimplePlaylist>();

            if (usernames == null) return usersPlaylists;

            if (usernames.Count() == 0) return usersPlaylists;

            foreach (var username in usernames)
            {
                int counter = 0;
                int limit = 20;

                var playlists = await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: 1, offset: 0);

                int total = playlists.Total;

                while (counter < total)
                {
                    var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: limit, offset: counter))?.Items;

                    counter += items.Count;

                    usersPlaylists.AddRange(items);
                }
            }

            return usersPlaylists;
        }

        public Task<ErrorResponse> PlayTrack(string trackUri)
        {
            return _spotifyWebAPI.ResumePlaybackAsync("", "", new List<string> { trackUri }, "", 0);
        }

        public Task<PublicProfile> GetMyProfile()
        {
            return _spotifyWebAPI.GetPublicProfileAsync(_privateProfile.Id);
        }

        [Obsolete]
        public async Task<FullPlaylist> RecreatePlaylist(string playlistUri, string playlistName, IEnumerable<string> ids)
        {
            FullPlaylist playlist = null;

            if (string.IsNullOrWhiteSpace(playlistUri) == false)
            {
                playlist = await _spotifyWebAPI.GetPlaylistAsync(userId: _privateProfile.Id, playlistId: playlistUri);

                var deleteTrackUries = (await GetPlaylistSongs(playlist.Id)).Select(x => new DeleteTrackUri(x.Track.Uri)).ToList();

                await _spotifyWebAPI.RemovePlaylistTracksAsync(_privateProfile.Id, playlist.Id, deleteTrackUries);

                //if (playlist.HasError() == false)
                    //playlist = await _spotifyWebAPI.CreatePlaylistAsync(_privateProfile.Id, playlistName);
            }
            else
            {
                playlist = await _spotifyWebAPI.CreatePlaylistAsync(_privateProfile.Id, playlistName);
            }

            var error = await _spotifyWebAPI.AddPlaylistTracksAsync(_privateProfile.Id, playlist.Id, ids.ToList());

            return playlist;
        }

        public async Task<IEnumerable<SimplePlaylist>> GlobalPlaylistsSearch(string query)
        {
            var foundPlaylists = new List<SimplePlaylist>();

            if (query == null) return foundPlaylists;

            int counter = 0;
            int limit = 20;
            int maxSize = 300;

            var searchResultPack = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Playlist, 1, 0, "");

            int total = searchResultPack.Playlists.Total;

            while (counter < total && counter < maxSize)
            {
                var searchResult = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Playlist, limit, counter, "");

                var items = searchResult.Playlists.Items;

                counter += items.Count;

                foundPlaylists.AddRange(items);
            }

            return foundPlaylists;
        }

        public async Task RemovePlaylistAsync(string playlistId)
        {
            var error = await _spotifyWebAPI.UnfollowPlaylistAsync(_privateProfile.Id, playlistId);
        }
    }
}
