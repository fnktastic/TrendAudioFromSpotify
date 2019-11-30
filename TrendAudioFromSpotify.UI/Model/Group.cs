using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Group : ViewModelBase
    {
        private readonly ISpotifyServices _spotifyServices;

        private bool readyToProcessing = false;
        private bool processingSpecificAudios = false;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        private string _top;
        public string Top
        {
            get { return _top; }
            set
            {
                if (value == _top) return;
                _top = value;
                RaisePropertyChanged(nameof(Top));
            }
        }

        private string _hitTreshold;
        public string HitTreshold
        {
            get { return _hitTreshold; }
            set
            {
                if (value == _hitTreshold) return;
                _hitTreshold = value;
                RaisePropertyChanged(nameof(HitTreshold));
            }
        }

        private ComparisonEnum _comparison;
        public ComparisonEnum Comparison
        {
            get { return _comparison; }
            set
            {
                if (value == _comparison) return;
                _comparison = value;
                RaisePropertyChanged(nameof(Comparison));
            }
        }

        private PlaylistTypeEnum _playlistType;
        public PlaylistTypeEnum PlaylistType
        {
            get { return _playlistType; }
            set
            {
                if (value == _playlistType) return;
                _playlistType = value;
                RaisePropertyChanged(nameof(PlaylistType));
            }
        }

        private bool _processingInProgress;
        public bool ProcessingInProgress
        {
            get { return _processingInProgress; }
            set
            {
                if (value == _processingInProgress) return;
                _processingInProgress = value;
                RaisePropertyChanged(nameof(ProcessingInProgress));
            }
        }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TimeSpan RefreshPeriod { get; set; }


        public virtual AudioCollection Audios { get; set; }
        public virtual PlaylistCollection Playlists { get; set; }

        private AudioCollection _trends;
        public AudioCollection Trends
        {
            get { return _trends; }
            set
            {
                if (value == _trends) return;
                _trends = value;
                RaisePropertyChanged(nameof(Trends));
            }
        }

        public Group()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Group(Group group, AudioCollection audios, PlaylistCollection playlists, ISpotifyServices spotifyServices)
        {
            _spotifyServices = spotifyServices;

            this.Name = group.Name;
            this.Top = group.Top;
            this.HitTreshold = group.HitTreshold;
            this.Comparison = group.Comparison;
            this.PlaylistType = group.PlaylistType;

            Audios = new AudioCollection(audios);
            Playlists = new PlaylistCollection(playlists);

            if (audios.Count > 0)
                processingSpecificAudios = true;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            readyToProcessing = true;
        }

        public void Process()
        {
            if (readyToProcessing)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProcessingInProgress = true;
                        });

                        var audiosOfPlaylists = new Dictionary<string, List<Audio>>();

                        foreach (var playlist in Playlists)
                        {
                            var audiosOfPlaylist = (await _spotifyServices.GetPlaylistSongs(playlist.Id)).Select(x => new Audio(x.Track)).ToList();

                            playlist.Audios = new AudioCollection(audiosOfPlaylist);

                            if (audiosOfPlaylists.ContainsKey(playlist.Id))
                                continue;

                            audiosOfPlaylists.Add(playlist.Id, audiosOfPlaylist);
                        }

                        var trendAudios = new Dictionary<Audio, int>();

                        var audioBunch = audiosOfPlaylists.Values.SelectMany(x => x).ToList();

                        if (processingSpecificAudios == false)
                            Audios = new AudioCollection(audiosOfPlaylists.Values.SelectMany(x => x).ToList());

                        var groupedAudios = audioBunch
                        .GroupBy(x => x.Id)
                        .Select(y =>
                        {
                            var audio = Audios.FirstOrDefault(z => z.Id == y.Key);

                            if (audio == null) return null;

                            audio.Hits = y.Count();

                            audio.Playlists = new PlaylistCollection();

                            foreach (var playlist in Playlists)
                            {
                                if (playlist.Audios.Any(x => string.Equals(x.Id, audio.Id, StringComparison.OrdinalIgnoreCase)))
                                    audio.Playlists.Add(playlist);
                            }

                            return audio;
                        })
                        .Where(x => x != null)
                        .ToList();

                        if (Comparison == ComparisonEnum.Equals)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits == int.Parse(_hitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(_top))
                            .ToList();
                        }

                        if (Comparison == ComparisonEnum.More)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits >= int.Parse(_hitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(_top))
                            .ToList();
                        }

                        if (Comparison == ComparisonEnum.Less)
                        {
                            groupedAudios = groupedAudios
                            .Where(x => x.Hits <= int.Parse(_hitTreshold))
                            .OrderByDescending(x => x.Hits)
                            .Take(int.Parse(_top))
                            .ToList();
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Trends = new AudioCollection(groupedAudios);
                        });
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProcessingInProgress = false;
                        });
                    }
                });
            }
        }
    }
}
