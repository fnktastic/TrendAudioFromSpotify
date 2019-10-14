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
    }
}
