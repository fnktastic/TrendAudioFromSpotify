using GalaSoft.MvvmLight.Messaging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<FullPlaylist> RecreatePlaylist(string playlistUri, string playlistName, IEnumerable<string> ids, bool isPublic = false);
        Task<PublicProfile> GetMyProfile();
        Task<PrivateProfile> GetPrivateProfile();
        Task<ErrorResponse> PlayTrack(string trackUri);
        Task<IEnumerable<SimplePlaylist>> GlobalPlaylistsSearch(string query);
        Task RemovePlaylistAsync(string playlistId);
        Task<PlaybackContext> GetCurrentlyPlaying();
        Task<FullPlaylist> GetPlaylistById(string playlistId);
        Task ChangePlaylistVisibility(string playlistId, bool newPublic);
        Task RemoveSongFromPlaylist(string playlistId, string songId);
        Task ReorderPlaylist(string playlistId, List<string> uris);
        Task SendToPlaylist(string playlistId, string audioId, int position);
        Task<IEnumerable<FullTrack>> GlobalAudiosSearch(string query);
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

        public async Task<FullPlaylist> GetPlaylistById(string playlistId)
        {
            var playlist = await _spotifyWebAPI.GetPlaylistAsync(playlistId: playlistId);

            return playlist;
        }

        public async Task<IEnumerable<SimplePlaylist>> GetAllPlaylists()
        {
            int counter = 0;

            int limit = 50;

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
                int limit = 50;

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

        public async Task<ErrorResponse> PlayTrack(string trackUri)
        {
            var playback = await _spotifyWebAPI.GetPlaybackAsync();

            var x = await _spotifyWebAPI.ResumePlaybackAsync(playback.Device?.Id, playback.Context?.Uri, new List<string> { trackUri }, "", 0);

            if(x.HasError())
            {

            }

            return x;
        }

        public Task<PublicProfile> GetMyProfile()
        {
            return _spotifyWebAPI.GetPublicProfileAsync(_privateProfile.Id);
        }

        public Task<PrivateProfile> GetPrivateProfile()
        {
            return _spotifyWebAPI.GetPrivateProfileAsync();
        }

        [Obsolete]
        public async Task<FullPlaylist> RecreatePlaylist(string playlistUri, string playlistName, IEnumerable<string> ids, bool isPublic = false)
        {
            FullPlaylist playlist = null;

            if (string.IsNullOrWhiteSpace(playlistUri) == false)
            {
                playlist = await _spotifyWebAPI.GetPlaylistAsync(playlistId: playlistUri);

                var deleteTrackUries = (await GetPlaylistSongs(playlist.Id)).Select(x => new DeleteTrackUri(x.Track.Uri)).ToList();

                await _spotifyWebAPI.RemovePlaylistTracksAsync(playlist.Id, deleteTrackUries);

                await RemovePlaylistAsync(playlist.Id);

                if (playlist.HasError() == false)
                    playlist = await _spotifyWebAPI.CreatePlaylistAsync(userId: _privateProfile.Id, playlistName: playlistName, isPublic: isPublic);
            }
            else
            {
                playlist = await _spotifyWebAPI.CreatePlaylistAsync(userId: _privateProfile.Id, playlistName: playlistName, isPublic: isPublic);
            }

            var error = await _spotifyWebAPI.AddPlaylistTracksAsync(playlist.Id, ids.ToList());

            return playlist;
        }

        public async Task<IEnumerable<SimplePlaylist>> GlobalPlaylistsSearch(string query)
        {
            var foundPlaylists = new List<SimplePlaylist>();

            if (query == null) return foundPlaylists;

            int counter = 0;
            int limit = 50;
            int maxSize = 2_000;

            var searchResultPack = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Playlist, limit: 1, offset: 0);

            if (searchResultPack.HasError())
                return new List<SimplePlaylist>();

            int total = searchResultPack.Playlists.Total;

            while (counter < total && counter < maxSize)
            {
                var searchResult = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Playlist, limit: limit, offset: counter);

                if (searchResult.HasError())
                    return foundPlaylists;

                var items = searchResult.Playlists.Items;

                counter += items.Count;

                foundPlaylists.AddRange(items);
            }

            return foundPlaylists;
        }

        public async Task<IEnumerable<FullTrack>> GlobalAudiosSearch(string query)
        {
            var foundTracks = new List<FullTrack>();

            if (query == null) return foundTracks;

            int counter = 0;
            int limit = 50;
            int maxSize = 2_000;

            var searchResultPack = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Track, limit: 1, offset: 0);

            if (searchResultPack.HasError())
                return new List<FullTrack>();

            int total = searchResultPack.Tracks.Total;

            while (counter < total && counter < maxSize)
            {
                var searchResult = await _spotifyWebAPI.SearchItemsEscapedAsync(query, Enums.SearchType.Track, limit: limit, offset: counter);

                if (searchResult.HasError())
                    return foundTracks;

                var items = searchResult.Tracks.Items;

                counter += items.Count;

                foundTracks.AddRange(items);
            }

            return foundTracks;
        }

        public async Task RemovePlaylistAsync(string playlistId)
        {
            var error = await _spotifyWebAPI.UnfollowPlaylistAsync(_privateProfile.Id, playlistId);
        }

        public async Task<PlaybackContext> GetCurrentlyPlaying()
        {
            var currentPlayback = await _spotifyWebAPI.GetPlaybackAsync();

            var currentlyPlsyingTrack = await _spotifyWebAPI.GetPlayingTrackAsync();

            return currentPlayback;
        }

        public async Task ChangePlaylistVisibility(string playlistId, bool newPublic)
        {
            await _spotifyWebAPI.UpdatePlaylistAsync(playlistId: playlistId, newPublic: newPublic);
        }

        public async Task RemoveSongFromPlaylist(string playlistId, string songId)
        {
            var deleteTrackUri = new DeleteTrackUri(songId);

            var x = await _spotifyWebAPI.RemovePlaylistTrackAsync(playlistId, deleteTrackUri);
        }

        public async Task ReorderPlaylist(string playlistId, List<string> uris)
        {
            var x = await _spotifyWebAPI.ReplacePlaylistTracksAsync(playlistId, uris);
        }

        public async Task SendToPlaylist(string playlistId, string audioId, int position)
        {
            var x = await _spotifyWebAPI.AddPlaylistTrackAsync(playlistId, audioId, position);
        }
    }
}
