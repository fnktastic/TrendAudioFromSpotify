using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Service.Spotify
{
    public interface ISpotifyServices
    {
        Task<IEnumerable<SimplePlaylist>> GetAllPlaylists();
        Task<IEnumerable<SavedTrack>> GetSongs();
        Task<IEnumerable<PlaylistTrack>> GetPlaylistSongs(string playlistId);
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(string username = "_annalasnier_");
        Task<IEnumerable<SimplePlaylist>> GetForeignUserPlaylists(IList<string> usernames);
        PrivateProfile PrivateProfile { get; }
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
            return (await _spotifyWebAPI.GetUserPlaylistsAsync(PrivateProfile.Id))?.Items;
        }

        public async Task<IEnumerable<SavedTrack>> GetSongs()
        {
            int counter = 0;
            int limit = 50;
            int total = (await _spotifyWebAPI.GetSavedTracksAsync()).Total;

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
            int total = (await _spotifyWebAPI.GetPlaylistTracksAsync(playlistId)).Total;

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
            int total = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, 20, 0)).Total;

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
                int total = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, 20, 0)).Total;

                while (counter < total)
                {
                    var items = (await _spotifyWebAPI.GetUserPlaylistsAsync(username, limit: limit, offset: counter))?.Items;

                    counter += items.Count;

                    usersPlaylists.AddRange(items);
                }
            }

            return usersPlaylists;
        }
    }
}
