using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IPlaylistService
    {
        Task<FullPlaylist> RecreateOnSpotify(Playlist sourcePlaylist);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly ISpotifyServices _spotifyServices;

        public PlaylistService(ISpotifyServices spotifyServices)
        {
            _spotifyServices = spotifyServices;
        }

        public async Task<FullPlaylist> RecreateOnSpotify(Playlist sourcePlaylist)
        {
            //add override, add fifo / typical type

            FullPlaylist playlist = null;

            if (sourcePlaylist.PlaylistType == PlaylistTypeEnum.Fifo)
            {
                playlist = await _spotifyServices.RecreatePlaylist(sourcePlaylist.SpotifyId, sourcePlaylist.Name, sourcePlaylist.Audios.Select(x => x.Uri));
            }
            if (sourcePlaylist.PlaylistType == PlaylistTypeEnum.Standard)
            {
                playlist = await _spotifyServices.RecreatePlaylist(sourcePlaylist.SpotifyId, sourcePlaylist.Name, sourcePlaylist.Audios.Select(x => x.Uri));
            }

            return playlist;
        }
    }
}
