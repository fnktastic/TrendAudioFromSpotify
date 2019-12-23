using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Collections;
using System.Windows;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IMonitoringService
    {
        MonitoringItem Initiate(ISpotifyServices spotifyServices, Group group, MonitoringItem monitoringItem, AudioCollection audios, PlaylistCollection playlists);
        Task<bool> ProcessAsync();
        bool IsMonitoringItemReady { get; }
    }

    public class MonitoringService : IMonitoringService
    {
        private ISpotifyServices _spotifyServices;
        private IDataService _dataService;
        private bool processingSpecificAudios = false;
        private Timer timer;
        public MonitoringItem MonitoringItem { get; set; }

        public MonitoringService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public MonitoringItem Initiate(ISpotifyServices spotifyServices, Group group, MonitoringItem monitoringItem, AudioCollection audios, PlaylistCollection playlists)
        {
            try
            {
                _spotifyServices = spotifyServices;

                this.MonitoringItem = new MonitoringItem();
                this.MonitoringItem.Group = new Group();

                this.MonitoringItem.Group.Id = Guid.NewGuid();
                this.MonitoringItem.Group.Name = group.Name;

                this.MonitoringItem.Id = Guid.NewGuid();
                this.MonitoringItem.Top = monitoringItem.Top;
                this.MonitoringItem.HitTreshold = monitoringItem.HitTreshold;
                this.MonitoringItem.Comparison = monitoringItem.Comparison;
                this.MonitoringItem.PlaylistType = monitoringItem.PlaylistType;
                this.MonitoringItem.RefreshPeriod = monitoringItem.RefreshPeriod;
                this.MonitoringItem.TargetPlaylistName = monitoringItem.TargetPlaylistName;
                this.MonitoringItem.AutoRecreatePlaylisOnSpotify = monitoringItem.AutoRecreatePlaylisOnSpotify;
                this.MonitoringItem.IsOverrideTrends = monitoringItem.IsOverrideTrends;

                MonitoringItem.Audios = new AudioCollection(audios);
                MonitoringItem.Playlists = new PlaylistCollection(playlists);
                MonitoringItem.Group.Playlists = new PlaylistCollection(playlists);

                if (audios.Count > 0)
                    processingSpecificAudios = true;

                MonitoringItem.CreatedAt = MonitoringItem.Group.CreatedAt = DateTime.UtcNow;
                MonitoringItem.UpdatedAt = MonitoringItem.Group.UpdatedAt = DateTime.UtcNow;

                if (playlists.Count > 0 && int.Parse(MonitoringItem.HitTreshold ?? "") > 0 && int.Parse(MonitoringItem.Top ?? "") > 0)
                    MonitoringItem.IsReady = true;

                return MonitoringItem;
            }
            catch
            {
                MonitoringItem.IsReady = false;

                return null;
            }
        }

        public bool IsMonitoringItemReady => MonitoringItem.IsReady;

        public async Task<bool> ProcessAsync()
        {
            if (MonitoringItem.IsReady)
            {
                await GetTrends();

                if (MonitoringItem.RefreshPeriod > TimeSpan.Zero)
                    RunTimer();

                return true;
            }

            return false;
        }

        private async Task GetTrends()
        {
            if (MonitoringItem.IsReady)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MonitoringItem.ProcessingInProgress = true;
                        });

                        var audiosOfPlaylists = new Dictionary<string, List<Audio>>();

                        foreach (var playlist in MonitoringItem.Playlists)
                        {
                            var audiosOfPlaylist = (await _spotifyServices.GetPlaylistSongs(playlist.Id)).Select(x => new Audio(x.Track)).Where(x => x.IsFilled).ToList();

                            playlist.Audios = new AudioCollection(audiosOfPlaylist);

                            if (audiosOfPlaylists.ContainsKey(playlist.Id))
                                continue;

                            audiosOfPlaylists.Add(playlist.Id, audiosOfPlaylist);
                        }

                        var trendAudios = new Dictionary<Audio, int>();

                        var audioBunch = audiosOfPlaylists.Values.SelectMany(x => x).Where(x => x.IsFilled).ToList();

                        if (processingSpecificAudios == false)
                            MonitoringItem.Audios = new AudioCollection(audiosOfPlaylists.Values.SelectMany(x => x).Where(x => x.IsFilled).ToList());

                        var groupedAudios = audioBunch
                        .GroupBy(x => x.Id)
                        .Select(y =>
                        {
                            var audio = MonitoringItem.Audios.FirstOrDefault(z => z.Id == y.Key);

                            if (audio == null) return null;

                            audio.Hits = y.Count();

                            audio.Playlists = new PlaylistCollection();

                            foreach (var playlist in MonitoringItem.Playlists)
                            {
                                if (playlist.Audios.Any(x => string.Equals(x.Id, audio.Id, StringComparison.OrdinalIgnoreCase)))
                                    audio.Playlists.Add(playlist);
                            }

                            return audio;
                        })
                        .Where(x => x != null)
                        .ToList();

                        if (MonitoringItem.Comparison == ComparisonEnum.Equals)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits == int.Parse(MonitoringItem.HitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(MonitoringItem.Top))
                            .ToList();
                        }

                        if (MonitoringItem.Comparison == ComparisonEnum.More)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits >= int.Parse(MonitoringItem.HitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(MonitoringItem.Top))
                            .ToList();
                        }

                        if (MonitoringItem.Comparison == ComparisonEnum.Less)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits <= int.Parse(MonitoringItem.HitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(MonitoringItem.Top))
                            .ToList();
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MonitoringItem.Trends = new AudioCollection(groupedAudios);
                        });

                        await SaveTrends(groupedAudios, MonitoringItem);

                        if (MonitoringItem.AutoRecreatePlaylisOnSpotify)
                            RecreateOnSpotify();
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MonitoringItem.ProcessingInProgress = false;
                        });
                    }
                });
            }
        }

        private async Task SaveTrends(IEnumerable<Audio> audios, MonitoringItem monitoringItem)
        {
            if (audios == null || audios.Count() == 0) return;

            await _dataService.InsertAudioRangeAsync(audios);
            await _dataService.InsertPlaylistAudioRangeAsync(audios);
            await _dataService.InsertMonitoringItemAudioRangeAsync(monitoringItem);
        }

        private void RunTimer()
        {
            timer = new Timer(async x => await GetTrends(), null, MonitoringItem.RefreshPeriod, MonitoringItem.RefreshPeriod);
        }

        private void RecreateOnSpotify()
        {
            if (MonitoringItem.PlaylistType == PlaylistTypeEnum.Fifo)
            {
                _spotifyServices.RecreatePlaylist(MonitoringItem.TargetPlaylistName, MonitoringItem.Trends.Select(x => x.Href));
            }

            if (MonitoringItem.PlaylistType == PlaylistTypeEnum.Standard)
            {
                _spotifyServices.RecreatePlaylist(MonitoringItem.TargetPlaylistName, MonitoringItem.Trends.Select(x => x.Uri));
            }
        }
    }
}
