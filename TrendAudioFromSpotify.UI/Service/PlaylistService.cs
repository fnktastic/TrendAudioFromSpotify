using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<List<Playlist>> BuildPlaylistAsync(MonitoringItem monitoringItem);
        Task ChangeVisibility(Playlist playlist, bool isPublic);
        Task RemoveSongFromPlaylist(Guid playlistId, string songId);
        Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition);
        Task SendToPlaylist(Audio audio, Guid playlistId, int newPosition);
        Task UpdatePlaylist(Playlist playlist);
        Task UpdatePlaylistTracks(Playlist playlist, IEnumerable<Audio> tracks);
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
            FullPlaylist playlist = null;

            var audioPositionDictionary = new Dictionary<string, int>(); 

            for (int i = 0; i < sourcePlaylist.Audios.Count; i++)
            {
                audioPositionDictionary.Add(sourcePlaylist.Audios.ElementAt(i).Uri, i);
            }
            
            playlist = await _spotifyServices.RecreatePlaylist(sourcePlaylist.SpotifyId, sourcePlaylist.DisplayName, audioPositionDictionary, sourcePlaylist.IsPublic);

            return playlist;
        }

        private async Task ClearPlaylists(MonitoringItem monitoringItem)
        {
            if (monitoringItem.IsSeries)
            {
                var playlists = await _dataService.GetPlaylistSeriesAsync(monitoringItem.TargetPlaylistName);

                foreach (var playlist in playlists)
                {
                    if (playlist.IsExported)
                    {
                        var songs = playlist.Audios.Select(x => x.Uri).ToList();

                        await _spotifyServices.ClearPlaylist(playlist.SpotifyId, songs);
                    }
                }
            }

            if (monitoringItem.IsSeries == false)
            {
                var playlist = await _dataService.GetPlaylistAsync(monitoringItem.TargetPlaylistName);
                if (playlist != null)
                {
                    if (playlist.IsExported)
                    {
                        var songs = playlist.Audios.Select(x => x.Uri).ToList();

                        await _spotifyServices.ClearPlaylist(playlist.SpotifyId, songs);
                    }
                }
            }
        }

        public async Task<List<Playlist>> BuildPlaylistAsync(MonitoringItem monitoringItem)
        {
            if(monitoringItem.IsDailyMonitoring)
            {
                return await BuildPlaylistsAsync(monitoringItem);
            }

            if (monitoringItem.IsOverridePlaylists == true) // override: replace playlists content
            {
                await ClearPlaylists(monitoringItem);

                return await BuildPlaylistsAsync(monitoringItem);
            }

            if (monitoringItem.IsOverridePlaylists == false)
            {
                return await BuildPlaylistsAsync(monitoringItem);
            }

            return new List<Playlist>();
        }

        private async Task<List<Playlist>> BuildPlaylistsAsync(MonitoringItem monitoringItem)
        {
            //step 1. Check for new Audios
            //get all audios of playlist (series, not-series)
            //except trends by existed audios

            var trends = monitoringItem.Trends;

            var audios = new List<Audio>();

            Playlist targetPlaylist = null;

            var playlistsToAddOrUpdate = new List<Playlist>();

            var existingVolumes = new List<Playlist>();

            if (monitoringItem.IsSeries == true)
            {
                existingVolumes = await _dataService.GetPlaylistSeriesAsync(monitoringItem.TargetPlaylistName);

                if (existingVolumes.Count > 0)
                {
                    audios = existingVolumes.SelectMany(x => x.Audios).ToList();

                    targetPlaylist = existingVolumes.OrderByDescending(x => x.SeriesNo).First();
                }
            }

            if (monitoringItem.IsSeries == false)
            {
                var playlist = await _dataService.GetPlaylistAsync(monitoringItem.TargetPlaylistName);

                if (playlist != null)
                {
                    audios = playlist.Audios.Select(x => x).ToList();

                    targetPlaylist = playlist;
                }
            }

            var newAudios = trends.ExceptBy(audios, x => x.Id).ToList();

            newAudios.ForEach(x => x.IsNew = true);

            int maxSize = int.Parse(monitoringItem.MaxSize);

            //fill up
            if (targetPlaylist != null && targetPlaylist.Audios.Count < maxSize)
            {
                int countToAppend = maxSize - targetPlaylist.Audios.Count;

                var itemsToAppend = newAudios.Take(countToAppend).ToList();

                newAudios = newAudios.Skip(countToAppend).ToList();

                if (monitoringItem.PlaylistType == PlaylistTypeEnum.Fifo)
                    itemsToAppend.ForEach(x => targetPlaylist.Audios.Insert(0, x));

                if (monitoringItem.PlaylistType == PlaylistTypeEnum.Standard)
                    itemsToAppend.ForEach(x => targetPlaylist.Audios.Add(x));

                targetPlaylist.Total = targetPlaylist.Audios.Count;
            }

            //if non-series - no need to continue. Playlists is full
            if (targetPlaylist != null && monitoringItem.IsSeries == false && targetPlaylist.Audios.Count >= maxSize)
            {
                await _dataService.InsertPlaylistAsync(targetPlaylist);
                playlistsToAddOrUpdate.Insert(0, targetPlaylist);
                return playlistsToAddOrUpdate;
            }

            //create new volumes
            Guid seriesKey = targetPlaylist == null ? Guid.NewGuid() : targetPlaylist.SeriesKey;
            int seriesNo = targetPlaylist == null ? 1 : targetPlaylist.SeriesNo + 1;
            int chunk = int.Parse(monitoringItem.MaxSize);
            int capacity = newAudios.Count;
            int counter = 0;

            while (counter < capacity)
            {
                var volumeAudios = newAudios.Skip(counter).Take(chunk).ToList();

                var volume = existingVolumes.FirstOrDefault(x => x.SeriesKey == seriesKey && x.SeriesNo == seriesNo);

                if (volume == null)
                    volume = new Playlist
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

                playlistsToAddOrUpdate.Add(volume);

                seriesNo++;
                counter += chunk;

                if (monitoringItem.IsSeries == false) break;
            }

            if (targetPlaylist != null)
                playlistsToAddOrUpdate.Insert(0, targetPlaylist);

            foreach (var playlistToAdd in playlistsToAddOrUpdate)
            {
                await _dataService.InsertPlaylistAsync(playlistToAdd);
            }

            return playlistsToAddOrUpdate;

            //step 2. Get already existed playlists OR Create them
            // if override -> clear playlists

            //step 3. Append new audios
            //append new audios to (if series - the last volume is target)
            // 1. the top of playlist (standard)
            // 2. the end of playlist (fifo)
        }

        [Obsolete]
        private async Task<List<Playlist>> BuildPlaylistWithoutOverridingAsync(MonitoringItem monitoringItem)
        {
            var trends = monitoringItem.Trends;

            if (monitoringItem.IsSeries == true)
            {
                var series = await _dataService.GetPlaylistSeriesAsync(monitoringItem.TargetPlaylistName);

                var seriesAudios = series.SelectMany(x => x.Audios).ToList();

                var newAudios = trends.ExceptBy(seriesAudios, x => x.Id).ToList();

                newAudios.ForEach(x => x.IsNew = true);

                await _dataService.RemovePlaylistSeriesPhysicallyAsync(monitoringItem.TargetPlaylistName);

                series.Clear();

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

                return series;
            }

            if (monitoringItem.IsSeries == false)
            {
                var playlist = await _dataService.GetPlaylistAsync(monitoringItem.TargetPlaylistName);

                if (playlist == null) // new playlist
                {
                    return await BuildPlaylistWithOverridingAsync(monitoringItem);
                }

                var playlistAudios = playlist.Audios.Select(x => x).ToList();

                var newAudios = trends.ExceptBy(playlistAudios, x => x.Id).ToList();

                newAudios.ForEach(x => x.IsNew = true);

                await _dataService.RemovePlaylistPhysicallyAsync(monitoringItem.TargetPlaylistName);

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

                playlistAudios = playlistAudios.Take(playlist.Total).ToList();

                playlist.Audios = new AudioCollection(playlistAudios);

                await _dataService.InsertPlaylistAsync(playlist);

                return new List<Playlist> { playlist };
            }

            return new List<Playlist>();
        }

        [Obsolete]
        private async Task<List<Playlist>> BuildPlaylistWithOverridingAsync(MonitoringItem monitoringItem)
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

                    volumeAudios.ForEach(x => x.IsNew = true);

                    volume.Audios = new AudioCollection(volumeAudios);

                    series.Add(volume);

                    await _dataService.InsertPlaylistAsync(volume);

                    seriesNo++;
                    counter += chunk;
                }

                return series;
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

                playlistAudios.ForEach(x => x.IsNew = true);

                playlist.Audios = new AudioCollection(playlistAudios);

                await _dataService.InsertPlaylistAsync(playlist);

                return new List<Playlist> { playlist };
            }

            return new List<Playlist>();
        }

        public async Task ChangeVisibility(Playlist playlist, bool isPublic)
        {
            await _dataService.ChangePlaylistVisibility(playlist, isPublic);
        }

        public async Task RemoveSongFromPlaylist(Guid playlistId, string songId)
        {
            await _dataService.RemoveSongFromPlaylist(playlistId, songId);
        }

        public async Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition)
        {
            return await _dataService.ChangeTrackPosition(playlistId, songId, oldPosition, newPosition);
        }

        public async Task SendToPlaylist(Audio audio, Guid playlistId, int newPosition)
        {
            await _dataService.SendToPlaylist(audio, playlistId, newPosition);
        }

        public async Task UpdatePlaylist(Playlist playlist)
        {
            await _dataService.UpdatePlaylist(playlist);
        }

        public async Task UpdatePlaylistTracks(Playlist playlist, IEnumerable<Audio> tracks)
        {
            await _dataService.ClearPlaylist(playlist).ContinueWith(async i =>
            {
                await _dataService.InsertAudioRangeAsync(tracks);

                await _dataService.InsertPlaylistAsync(playlist);
            });
        }
    }
}
