using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enums = SpotifyAPI.Web.Enums;

namespace TrendAudioFromSpotify.Service.Spotify
{
    public interface ISpotifyServices
    {
        Task<IEnumerable<SimplePlaylist>> GetAllPlaylists();
        Task<IEnumerable<SavedTrack>> GetSongs();
        Task<IEnumerable<PlaylistTrack>> GetPlaylistSongs(string playlistId);
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(string username = "_annalasnier_");
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(IList<string> usernames);
        Task RecreatePlaylist(string playlistName, IEnumerable<string> ids);
        Task<PublicProfile> GetMyProfile();
        Task<ErrorResponse> PlayTrack(string trackUri);
        PrivateProfile PrivateProfile { get; }
        Task<IEnumerable<SimplePlaylist>> GlobalPlaylistsSearch(string query);
    }

    public class SpotifyServices : ISpotifyServices
    {
        private SpotifyWebAPI _spotifyWebAPI;

        public PrivateProfile PrivateProfile { get; private set; }

        public SpotifyServices(SpotifyWebAPI spotifyWebAPI)
        {
            _spotifyWebAPI = spotifyWebAPI;
            PrivateProfile = _spotifyWebAPI.GetPrivateProfile();
        }

        public async Task<IEnumerable<SimplePlaylist>> GetAllPlaylists()
        {
            int counter = 0;

            int limit = 20;

            var playlists = await _spotifyWebAPI.GetUserPlaylistsAsync(PrivateProfile.Id, limit: 1, offset: 0);

            if (playlists.Error != null)
            {
                SpotifyProvider.Auth();
            }

            int total = playlists.Total;

            var usersPlaylists = new List<SimplePlaylist>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(PrivateProfile.Id, limit: limit, offset: counter))?.Items;

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
                SpotifyProvider.Auth();
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
                SpotifyProvider.Auth();
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
                SpotifyProvider.Auth();
            }

            int total = playlists.Total;

            var usersPlaylists = new List<SimplePlaylist>();

            while (counter < total)
            {
                var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: limit, offset: counter))?.Items;

                counter += items.Count; ;

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
            return _spotifyWebAPI.GetPublicProfileAsync(PrivateProfile.Id);
        }

        [Obsolete]
        public async Task RecreatePlaylist(string playlistName, IEnumerable<string> ids)
        {
            var playlist = await _spotifyWebAPI.CreatePlaylistAsync(PrivateProfile.Id, playlistName);

            var error = await _spotifyWebAPI.AddPlaylistTracksAsync(PrivateProfile.Id, playlist.Id, ids.ToList());
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
    }
}
