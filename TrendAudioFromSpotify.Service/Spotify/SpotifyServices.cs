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
        Task<IEnumerable<SavedTrack>> GetSongs(int take = 50, int offset = 0);
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

        public async Task<IEnumerable<SavedTrack>> GetSongs(int take = 50, int offset = 0)
        {
            return (await _spotifyWebAPI.GetSavedTracksAsync(take, offset))?.Items;
        }

        [Obsolete]
        public async Task<IEnumerable<PlaylistTrack>> GetPlaylistSongs(string playlistId)
        {
            return (await _spotifyWebAPI.GetPlaylistTracksAsync(playlistId))?.Items;
        }
    }
}
