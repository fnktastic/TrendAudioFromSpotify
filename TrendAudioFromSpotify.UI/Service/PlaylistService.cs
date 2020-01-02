using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Extensions;
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
                playlist = await _spotifyServices.RecreatePlaylist(sourcePlaylist.SpotifyId, sourcePlaylist.DisplayName, sourcePlaylist.Audios.Select(x => x.Uri));
            }
            if (sourcePlaylist.PlaylistType == PlaylistTypeEnum.Standard)
            {
                playlist = await _spotifyServices.RecreatePlaylist(sourcePlaylist.SpotifyId, sourcePlaylist.DisplayName, sourcePlaylist.Audios.Select(x => x.Uri));
            }

            return playlist;
        }

        public async Task BuildPlaylistAsync(MonitoringItem monitoringItem)
        {
            if (monitoringItem.IsOverrideTrends == true) // override: remove existed playlists
            {
                await BuildPlaylistWithOverridingAsync(monitoringItem);
            }

            if (monitoringItem.IsOverrideTrends == false)
            {
                await BuildPlaylistWithoutOverridingAsync(monitoringItem);
            }
        }

        private async Task BuildPlaylistWithoutOverridingAsync(MonitoringItem monitoringItem)
        {
            var trends = monitoringItem.Trends;

            if (monitoringItem.IsSeries == true)
            {
                var series = await _dataService.GetPlaylistSeriesAsync(monitoringItem.TargetPlaylistName);

                var seriesAudios = series.SelectMany(x => x.Audios).ToList();

                var newAudios = trends.ExceptBy(seriesAudios, x => x.Id).ToList();

                await _dataService.RemovePlaylistSeriesPhysicallyAsync(monitoringItem.TargetPlaylistName);

                //if (newAudios.Count > 0) // if we have new items
                {
                    if (monitoringItem.PlaylistType == PlaylistTypeEnum.Fifo)
                    {
                        seriesAudios.InsertRange(0, newAudios);
                    }

                    if (monitoringItem.PlaylistType == PlaylistTypeEnum.Standard)
                    {
                        seriesAudios.AddRange(newAudios);
                    }

                    Guid seriesKey = Guid.NewGuid();
                    int seriesNo = 1;
                    int chunk = int.Parse(monitoringItem.MaxSize);
                    int capacity = seriesAudios.Count;
                    int counter = 0;

                    while (counter < capacity)
                    {
                        var volumeAudios = seriesAudios.Skip(counter).Take(chunk).ToList();

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
            }

            if (monitoringItem.IsSeries == false)
            {
                var playlist = await _dataService.GetPlaylistAsync(monitoringItem.TargetPlaylistName);

                if (playlist == null) // new playlist
                {
                    await BuildPlaylistWithOverridingAsync(monitoringItem);
                    return;
                }

                var playlistAudios = playlist.Audios.Select(x => x).ToList();

                var newAudios = trends.ExceptBy(playlistAudios, x => x.Id).ToList();

                await _dataService.RemovePlaylistPhysicallyAsync(monitoringItem.TargetPlaylistName);

                //if (newAudios.Count > 0)
                {
                    if (monitoringItem.PlaylistType == PlaylistTypeEnum.Fifo)
                    {
                        playlistAudios.InsertRange(0, newAudios);
                    }
                    if (monitoringItem.PlaylistType == PlaylistTypeEnum.Standard)
                    {
                        playlistAudios.AddRange(newAudios);
                    }

                    var newPlaylist = new Playlist
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

                    var newPlaylistAudios = playlistAudios.Take(playlist.Total).ToList();

                    playlist.Audios = new AudioCollection(playlistAudios);

                    await _dataService.InsertPlaylistAsync(playlist);
                }
            }
        }

        private async Task BuildPlaylistWithOverridingAsync(MonitoringItem monitoringItem)
        {
            var trends = monitoringItem.Trends;

            if (monitoringItem.IsSeries == false)
                await _dataService.RemovePlaylistPhysicallyAsync(monitoringItem.TargetPlaylistName);

            if (monitoringItem.IsSeries)
                await _dataService.RemovePlaylistSeriesPhysicallyAsync(monitoringItem.TargetPlaylistName);

            var series = new List<Playlist>();

            if (monitoringItem.IsSeries == true)
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

                await _dataService.InsertPlaylistAsync(playlist);
            }
        }
    }
}
