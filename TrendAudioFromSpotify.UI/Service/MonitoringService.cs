using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Extensions;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IMonitoringService
    {
        MonitoringItem Initiate(Group group, MonitoringItem monitoringItem, PlaylistCollection playlists);
        Task<bool> ProcessAsync(MonitoringItem monitoringItem);
    }

    public class MonitoringService : IMonitoringService
    {
        private const int MAX_SIZE = 300;
        private readonly ISchedulingService _schedulingService;
        private readonly ISpotifyServices _spotifyServices;
        private readonly IDataService _dataService;
        private readonly IPlaylistService _playlistService;

        public MonitoringService(IDataService dataService, ISpotifyServices spotifyServices, ISchedulingService schedulingService, IPlaylistService playlistService)
        {
            _dataService = dataService;
            _spotifyServices = spotifyServices;
            _schedulingService = schedulingService;
            _playlistService = playlistService;
        }

        public MonitoringItem Initiate(Group group, MonitoringItem monitoringItem, PlaylistCollection playlists)
        {
            MonitoringItem _monitoringItem = new MonitoringItem();

            try
            {
                _monitoringItem = new MonitoringItem();
                _monitoringItem.Group = new Group();

                _monitoringItem.Group.Id = group.Id == Guid.Empty ? Guid.NewGuid() : group.Id;
                _monitoringItem.Group.Name = group.Name;

                _monitoringItem.Id = monitoringItem.Id == Guid.Empty ? Guid.NewGuid() : monitoringItem.Id;
                _monitoringItem.MaxSize = monitoringItem.MaxSize;
                _monitoringItem.HitTreshold = monitoringItem.HitTreshold;
                _monitoringItem.Comparison = monitoringItem.Comparison;
                _monitoringItem.PlaylistType = monitoringItem.PlaylistType;
                _monitoringItem.Schedule = monitoringItem.Schedule;
                _monitoringItem.TargetPlaylistName = monitoringItem.TargetPlaylistName;
                _monitoringItem.AutoRecreatePlaylisOnSpotify = monitoringItem.AutoRecreatePlaylisOnSpotify;
                _monitoringItem.IsOverrideTrends = monitoringItem.IsOverrideTrends;
                _monitoringItem.IsSeries = monitoringItem.IsSeries;
                _monitoringItem.TrendsSorting = monitoringItem.TrendsSorting;
                _monitoringItem.IsOverridePlaylists = monitoringItem.IsOverridePlaylists;
                _monitoringItem.IsDailyTrends = monitoringItem.IsDailyTrends;
                _monitoringItem.IsRandomizeGroup = monitoringItem.IsRandomizeGroup;

                _monitoringItem.Group.Playlists = new PlaylistCollection(playlists);

                _monitoringItem.CreatedAt = _monitoringItem.Group.CreatedAt = DateTime.UtcNow;
                _monitoringItem.UpdatedAt = _monitoringItem.Group.UpdatedAt = DateTime.UtcNow;

                if (playlists.Count > 0 && int.Parse(_monitoringItem.HitTreshold ?? "") > 0 && int.Parse(monitoringItem.MaxSize ?? "") > 0)
                    _monitoringItem.IsReady = true;

                return _monitoringItem;
            }
            catch
            {
                _monitoringItem.IsReady = false;

                return null;
            }
        }

        public async Task<bool> ProcessAsync(MonitoringItem monitoringItem)
        {
            if (monitoringItem.IsReady)
            {
                monitoringItem.Group = _dataService.GetFreshGroup(monitoringItem.Group);

                await GetTrends(monitoringItem);

                if (monitoringItem.Schedule.RepeatOn == true)
                    await StartSchedulingTimer(monitoringItem);

                return true;
            }

            return false;
        }

        private async Task GetTrends(MonitoringItem monitoringItem)
        {
            if (monitoringItem.IsReady)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            monitoringItem.ProcessingInProgress = true;
                        });

                        var audiosOfPlaylists = new Dictionary<string, List<Audio>>();

                        if (monitoringItem.IsRandomizeGroup)
                            monitoringItem.Group.Playlists.Shuffle();

                        foreach (var playlist in monitoringItem.Group.Playlists)
                        {
                            var audiosOfPlaylist = (await _spotifyServices.GetPlaylistSongs(playlist.SpotifyId))
                            .Where(x => x != null && x.Track != null)
                            .Select(x => new Audio(x.Track))
                            .Where(x => x.IsFilled)
                            .ToList();

                            playlist.Audios = new AudioCollection(audiosOfPlaylist);

                            if (audiosOfPlaylists.ContainsKey(playlist.SpotifyId))
                                continue;

                            audiosOfPlaylists.Add(playlist.SpotifyId, audiosOfPlaylist);

                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }

                        var trendAudios = new Dictionary<Audio, int>();

                        var audioBunch = audiosOfPlaylists.Values.SelectMany(x => x).Where(x => x.IsFilled).ToList();

                        var groupedAudios = audioBunch
                        .GroupBy(x => x.Id)
                        .Select(y =>
                        {
                            var audio = audioBunch.FirstOrDefault(z => z.Id == y.Key);

                            if (audio == null) return null;

                            audio.Hits = y.Count();

                            audio.Playlists = new PlaylistCollection();

                            foreach (var playlist in monitoringItem.Group.Playlists)
                            {
                                if (playlist.Audios.Any(x => string.Equals(x.Id, audio.Id, StringComparison.OrdinalIgnoreCase)))
                                    audio.Playlists.Add(playlist);
                            }

                            return audio;
                        })
                        .Where(x => x != null)
                        .ToList();

                        if (monitoringItem.Comparison == ComparisonEnum.Equals)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits == int.Parse(monitoringItem.HitTreshold))
                            .Take(MAX_SIZE)
                            .ToList();
                        }

                        if (monitoringItem.Comparison == ComparisonEnum.More)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits >= int.Parse(monitoringItem.HitTreshold))
                            .Take(MAX_SIZE)
                            .ToList();
                        }

                        if (monitoringItem.Comparison == ComparisonEnum.Less)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits <= int.Parse(monitoringItem.HitTreshold))
                            .Take(MAX_SIZE)
                            .ToList();
                        }

                        groupedAudios = MixTrends(monitoringItem.TrendsSorting, groupedAudios);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            monitoringItem.Trends = new AudioCollection(groupedAudios);
                        });

                        await SaveTrends(monitoringItem.Group.Playlists, monitoringItem);
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            monitoringItem.ProcessingInProgress = false;
                        });
                    }
                });
            }
        }

        private List<Audio> MixTrends(TrendsSortingEnum trendsSorting, List<Audio> trends)
        {
            switch(trendsSorting)
            {
                case TrendsSortingEnum.None:
                    return trends;
                case TrendsSortingEnum.Popularity:
                    return trends.OrderByDescending(x => x.Popularity).ToList();
                case TrendsSortingEnum.Random:
                    trends.Shuffle();
                    return trends;
                case TrendsSortingEnum.Hits:
                    return trends.OrderByDescending(x => x.Hits).ToList();
                default:
                    return trends;
            }
        }

        private async Task SaveTrends(IEnumerable<Playlist> playlists, MonitoringItem monitoringItem)
        {
            if (monitoringItem.Trends == null || monitoringItem.Trends.Count() == 0) return;

            await _dataService.InsertAudioRangeAsync(monitoringItem.Trends);
            await _dataService.InsertPlaylistAudioRangeAsync(monitoringItem.Trends);
            await _dataService.InsertMonitoringItemAsync(monitoringItem);

            if(monitoringItem.IsOverrideTrends)
            {
                //delete from DB existed trends of MI
                await _dataService.RemoveTrendsFromMonitoringItem(monitoringItem);
            }

            if (monitoringItem.IsOverrideTrends == false)
            {
                var trends = await _dataService.GetMonitoringItemAudios(monitoringItem);

                trends = trends.ExceptBy(monitoringItem.Trends, x => x.Uri).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    trends.ForEach(x => monitoringItem.Trends.Insert(0, x));
                });
            }

            await _dataService.InsertMonitoringItemAudioRangeAsync(monitoringItem);

            var builtPlaylists = await _playlistService.BuildPlaylistAsync(monitoringItem);

            Messenger.Default.Send<PlaylistBuiltMessage>(new PlaylistBuiltMessage(monitoringItem, builtPlaylists));
        }

        private async Task StartSchedulingTimer(MonitoringItem monitoringItem)
        {
            if (monitoringItem.Schedule.RepeatOn)
                await _schedulingService.ScheduleMonitoringItem(monitoringItem);
        }
    }
}
