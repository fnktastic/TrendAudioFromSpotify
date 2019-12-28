using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IPlaylistService
    {
        Task<FullPlaylist> RecreateOnSpotify(Playlist sourcePlaylist);
        Task BuildPlaylistAsync(MonitoringItem monitoringItem);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly ISpotifyServices _spotifyServices;
        private readonly IDataService _dataService;

        public PlaylistService(ISpotifyServices spotifyServices, IDataService dataService)
        {
            _spotifyServices = spotifyServices;
            _dataService = dataService;
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

        public async Task BuildPlaylistAsync(MonitoringItem monitoringItem)
        {
            var trends = monitoringItem.Trends;

            if (monitoringItem.IsOverrideTrends) // override: remove existed playlists
            {
                if (monitoringItem.IsSeries == false)
                    await _dataService.RemovePlaylistAsync(monitoringItem.TargetPlaylistName);

                if (monitoringItem.IsSeries)
                    await _dataService.RemovePlaylistSeriesAsync(monitoringItem.TargetPlaylistName);
            }

            var series = new List<Playlist>();

            if (monitoringItem.IsSeries)
            {
                Guid seriesKey = Guid.NewGuid();
                int seriesNo = 1;
                int chunk = int.Parse(monitoringItem.MaxSize);
                int capacity = trends.Count;
                int counter = 0;

                while (counter < capacity)
                {
                    var volumeAudios = trends.Skip(counter).Take(chunk).ToList();

                    var volume = new Playlist
                    {
                        Name = monitoringItem.TargetPlaylistName,
                        IsSeries = monitoringItem.IsSeries,
                        SeriesKey = seriesKey,
                        SeriesNo = seriesNo,
                        MadeByUser = true,
                        Total = volumeAudios.Count,
                        PlaylistType = monitoringItem.PlaylistType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    volume.Audios = new AudioCollection(volumeAudios);

                    series.Add(volume);

                    await _dataService.InsertPlaylistAsync(volume);

                    seriesNo++;
                    counter += chunk;
                }
            }

            if (monitoringItem.IsSeries == false)
            {
                var playlist = new Playlist
                {
                    Name = monitoringItem.TargetPlaylistName,
                    IsSeries = monitoringItem.IsSeries,
                    MadeByUser = true,
                    SeriesKey = Guid.NewGuid(),
                    SeriesNo = 0,
                    Total = int.Parse(monitoringItem.MaxSize),
                    PlaylistType = monitoringItem.PlaylistType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var playlistAudios = trends.Take(playlist.Total).ToList();

                playlist.Audios = new AudioCollection(playlistAudios);

                series.Add(playlist);

                await _dataService.InsertPlaylistAsync(playlist);
            }
        }
    }
}
