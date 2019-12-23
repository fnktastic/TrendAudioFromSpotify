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
        Task<bool> ProcessAsync(MonitoringItem monitoringItem);
    }

    public class MonitoringService : IMonitoringService
    {
        private ISpotifyServices _spotifyServices;
        private IDataService _dataService;
        private bool processingSpecificAudios = false;
        private Timer timer;

        public MonitoringService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public MonitoringItem Initiate(ISpotifyServices spotifyServices, Group group, MonitoringItem monitoringItem, AudioCollection audios, PlaylistCollection playlists)
        {
            MonitoringItem _monitoringItem = new MonitoringItem();

            try
            {
                _spotifyServices = spotifyServices;

                _monitoringItem = new MonitoringItem();
                _monitoringItem.Group = new Group();

                _monitoringItem.Group.Id = group.Id == Guid.Empty ? Guid.NewGuid() : group.Id;
                _monitoringItem.Group.Name = group.Name;

                _monitoringItem.Id = Guid.NewGuid();
                _monitoringItem.Top = monitoringItem.Top;
                _monitoringItem.HitTreshold = monitoringItem.HitTreshold;
                _monitoringItem.Comparison = monitoringItem.Comparison;
                _monitoringItem.PlaylistType = monitoringItem.PlaylistType;
                _monitoringItem.RefreshPeriod = monitoringItem.RefreshPeriod;
                _monitoringItem.TargetPlaylistName = monitoringItem.TargetPlaylistName;
                _monitoringItem.AutoRecreatePlaylisOnSpotify = monitoringItem.AutoRecreatePlaylisOnSpotify;
                _monitoringItem.IsOverrideTrends = monitoringItem.IsOverrideTrends;

                _monitoringItem.Audios = new AudioCollection(audios);
                _monitoringItem.Playlists = new PlaylistCollection(playlists);
                _monitoringItem.Group.Playlists = new PlaylistCollection(playlists);

                if (audios.Count > 0)
                    processingSpecificAudios = true;

                _monitoringItem.CreatedAt = _monitoringItem.Group.CreatedAt = DateTime.UtcNow;
                _monitoringItem.UpdatedAt = _monitoringItem.Group.UpdatedAt = DateTime.UtcNow;

                if (playlists.Count > 0 && int.Parse(_monitoringItem.HitTreshold ?? "") > 0 && int.Parse(monitoringItem.Top ?? "") > 0)
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
                await GetTrends(monitoringItem);

                if (monitoringItem.RefreshPeriod > TimeSpan.Zero)
                    RunTimer(monitoringItem);

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

                        foreach (var playlist in monitoringItem.Playlists)
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
                            monitoringItem.Audios = new AudioCollection(audiosOfPlaylists.Values.SelectMany(x => x).Where(x => x.IsFilled).ToList());

                        var groupedAudios = audioBunch
                        .GroupBy(x => x.Id)
                        .Select(y =>
                        {
                            var audio = monitoringItem.Audios.FirstOrDefault(z => z.Id == y.Key);

                            if (audio == null) return null;

                            audio.Hits = y.Count();

                            audio.Playlists = new PlaylistCollection();

                            foreach (var playlist in monitoringItem.Playlists)
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
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(monitoringItem.Top))
                            .ToList();
                        }

                        if (monitoringItem.Comparison == ComparisonEnum.More)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits >= int.Parse(monitoringItem.HitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(monitoringItem.Top))
                            .ToList();
                        }

                        if (monitoringItem.Comparison == ComparisonEnum.Less)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits <= int.Parse(monitoringItem.HitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(monitoringItem.Top))
                            .ToList();
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            monitoringItem.Trends = new AudioCollection(groupedAudios);
                        });

                        await SaveTrends(groupedAudios, monitoringItem);

                        if (monitoringItem.AutoRecreatePlaylisOnSpotify)
                            RecreateOnSpotify(monitoringItem);
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

        private async Task SaveTrends(IEnumerable<Audio> audios, MonitoringItem monitoringItem)
        {
            if (audios == null || audios.Count() == 0) return;

            await _dataService.InsertAudioRangeAsync(audios);
            await _dataService.InsertPlaylistAudioRangeAsync(audios);
            await _dataService.InsertMonitoringItemAudioRangeAsync(monitoringItem);
        }

        private void RunTimer(MonitoringItem monitoringItem)
        {
            timer = new Timer(async x => await GetTrends(monitoringItem), null, monitoringItem.RefreshPeriod, monitoringItem.RefreshPeriod);
        }

        private void RecreateOnSpotify(MonitoringItem monitoringItem)
        {
            if (monitoringItem.PlaylistType == PlaylistTypeEnum.Fifo)
            {
                _spotifyServices.RecreatePlaylist(monitoringItem.TargetPlaylistName, monitoringItem.Trends.Select(x => x.Href));
            }

            if (monitoringItem.PlaylistType == PlaylistTypeEnum.Standard)
            {
                _spotifyServices.RecreatePlaylist(monitoringItem.TargetPlaylistName, monitoringItem.Trends.Select(x => x.Uri));
            }
        }
    }
}
