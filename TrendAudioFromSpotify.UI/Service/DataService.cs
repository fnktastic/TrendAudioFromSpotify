using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.Model;
using TrendAudioFromSpotify.Data.Repository;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IDataService
    {
        Task InsertAudioAsync(Audio audio);
        Task InsertMonitoringItemAsync(MonitoringItem monitoringItem);
        Task InsertPlaylistRangeAsync(IEnumerable<Playlist> playlists);
        Task InsertMonitoringItemAudioRangeAsync(MonitoringItem monitoringItem);
        Task InsertGroupPlaylistRangeAsync(Group group);
        Task InsertGroupAsync(Group group);
        Task<List<Group>> GetAllGroupsAsync();
        Task InsertAudioRangeAsync(IEnumerable<Audio> audios);
        Task<List<MonitoringItem>> GetAllMonitoringItemsAsync();
        Task<List<Audio>> GetAllMonitoringItemAudioByMonitoringItemIdAsync(Guid monitoringItemId);
        Task InsertPlaylistAudioRangeAsync(IEnumerable<Audio> audios);
        Task RemoveGroupAsync(Group group);
        Task RemoveMonitoringItemAsync(MonitoringItem monitoringItem);
        Task AddSpotifyUriHrefToPlaylistAsync(Guid id, string playlistId, string playlistHref, string playlistUri, string playlistOwner, string playlistOwnerProfileUrl, string playlistCover);
        Task<List<Playlist>> GetAllPlaylistsAsync(bool madeByUser = true);
        Task RemovePlaylistAsync(Playlist playlist);
        Task RemovePlaylistPhysicallyAsync(string playlistName);
        Task RemovePlaylistSeriesPhysicallyAsync(string seriesName);
        Task InsertPlaylistAsync(Playlist playlist);
        Task<List<Playlist>> GetPlaylistSeriesAsync(string seriesName);
        Task<Playlist> GetPlaylistAsync(string playlistName);
        Task<List<Playlist>> GetPlaylistsByMonitoringItemAsync(MonitoringItem monitoringItem);
        Task<MonitoringItem> GetMonitoringItemByIdAsync(Guid monitoringItemId);
        Task ChangePlaylistVisibility(Playlist playlist, bool isPublic);
        Task RemoveSongFromPlaylist(Guid playlistId, string songId);
        Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition);
    }

    public class DataService : IDataService
    {
        private readonly SerialQueue _serialQueue;
        private readonly IMapper _mapper;
        private readonly IAudioRepository _audioRepository;
        private readonly IGroupPlaylistRepository _groupPlaylistRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IPlaylistAudioRepository _playlistAudioRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IMonitoringItemRepository _monitoringItemRepository;
        private readonly IMonitoringItemAudioRepository _monitoringItemAudioRepository;

        public DataService(
            SerialQueue serialQueue,
            IMapper mapper,
            IAudioRepository audioRepository,
            IGroupPlaylistRepository groupPlaylistRepository,
            IGroupRepository groupRepository,
            IPlaylistAudioRepository playlistAudioRepository,
            IPlaylistRepository playlistRepository,
            IMonitoringItemRepository monitoringItemRepository,
            IMonitoringItemAudioRepository monitoringItemAudioRepository
            )
        {
            _serialQueue = serialQueue;
            _mapper = mapper;
            _audioRepository = audioRepository;
            _groupPlaylistRepository = groupPlaylistRepository;
            _groupRepository = groupRepository;
            _playlistAudioRepository = playlistAudioRepository;
            _playlistRepository = playlistRepository;
            _monitoringItemRepository = monitoringItemRepository;
            _monitoringItemAudioRepository = monitoringItemAudioRepository;
        }

        public async Task InsertAudioAsync(Audio audio)
        {
            await _serialQueue.Enqueue(async () => await _audioRepository.InsertAsync(_mapper.Map<AudioDto>(audio)));
        }

        public async Task InsertMonitoringItemAsync(MonitoringItem monitoringItem)
        {
            await _serialQueue.Enqueue(async () => await _monitoringItemRepository.InsertAsync(_mapper.Map<MonitoringItemDto>(monitoringItem)));
        }

        public async Task InsertPlaylistRangeAsync(IEnumerable<Playlist> playlists)
        {
            await _serialQueue.Enqueue(async () => await _playlistRepository.InsertRangeAsync(_mapper.Map<List<PlaylistDto>>(playlists)));
        }

        public async Task InsertGroupAsync(Group group)
        {
            await _serialQueue.Enqueue(async () => await _groupRepository.InsertAsync(_mapper.Map<GroupDto>(group)));
        }

        public async Task InsertGroupPlaylistRangeAsync(Group group)
        {
            await _serialQueue.Enqueue(async () =>
            {
                await _groupPlaylistRepository.InsertRangeAsync(group.Playlists.Select(x => new GroupPlaylistDto()
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    GroupId = group.Id,
                    PlaylistId = x.Id
                }).ToList());
            });
        }

        public async Task InsertMonitoringItemAudioRangeAsync(MonitoringItem monitoringItem)
        {
            await _serialQueue.Enqueue(async () =>
            {
                await _monitoringItemAudioRepository.InsertRangeAsync(monitoringItem.Trends.Select(x => new MonitoringItemAudioDto()
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    MonitoringItemId = monitoringItem.Id,
                    AudioId = x.Id,
                    Hits = x.Hits
                }).ToList());
            });
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var groups = await _serialQueue.Enqueue(async () => await _groupRepository.GetAllAsync());

            return _mapper.Map<List<Group>>(groups);
        }

        public async Task InsertAudioRangeAsync(IEnumerable<Audio> audios)
        {
            await _serialQueue.Enqueue(async () => await _audioRepository.InsertAudioRangeAsync(_mapper.Map<IEnumerable<AudioDto>>(audios)));
        }

        public async Task<List<MonitoringItem>> GetAllMonitoringItemsAsync()
        {
            var monitoringItems = await _serialQueue.Enqueue(async () => await _monitoringItemRepository.GetAllAsync());

            return _mapper.Map<List<MonitoringItem>>(monitoringItems);
        }

        public async Task<List<Audio>> GetAllMonitoringItemAudioByMonitoringItemIdAsync(Guid monitoringItemId)
        {
            var monitoringItemAudios = await _serialQueue.Enqueue(async () => await _monitoringItemAudioRepository.GetAllByMonitoringItemIdAsync(monitoringItemId));

            var audios = _mapper.Map<List<Audio>>(monitoringItemAudios.Select(x => x.Audio));

            audios.ForEach(x => x.Hits = monitoringItemAudios.First(y => y.AudioId == x.Id).Hits);

            return audios.OrderByDescending(x => x.Hits).ToList();
        }

        public async Task InsertPlaylistAudioRangeAsync(IEnumerable<Audio> audios)
        {
            var playlistAudioDtos = new List<PlaylistAudioDto>();

            foreach (var audio in audios)
            {
                playlistAudioDtos.AddRange(audio.Playlists.Select(playlist => new PlaylistAudioDto()
                {
                    AudioId = audio.Id,
                    PlaylistId = playlist.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }));
            }

            await _serialQueue.Enqueue(async () => await _playlistAudioRepository.InsertPlaylistAudioRangeAsync(playlistAudioDtos));
        }

        public async Task RemoveGroupAsync(Group group)
        {
            var groupDto = _mapper.Map<GroupDto>(group);

            await _serialQueue.Enqueue(async () => await _groupRepository.RemoveAsync(groupDto));
        }

        public async Task RemoveMonitoringItemAsync(MonitoringItem monitoringItem)
        {
            var monitoringItemDto = _mapper.Map<MonitoringItemDto>(monitoringItem);

            await _serialQueue.Enqueue(async () => await _monitoringItemRepository.RemoveAsync(monitoringItemDto));
        }

        public async Task AddSpotifyUriHrefToPlaylistAsync(Guid id, string playlistId, string playlistHref, string playlistUri, string playlistOwner, string playlistOwnerProfileUrl, string playlistCover)
        {
            await _serialQueue.Enqueue(async () => await _playlistRepository.AddSpotifyUriHrefAsync(id, playlistId, playlistHref, playlistUri, playlistOwner, playlistOwnerProfileUrl, playlistCover));
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(bool madeByUser = true)
        {
            var playlists = await _serialQueue.Enqueue(async () => await _playlistRepository.GetAllAsync(madeByUser));

            return _mapper.Map<List<Playlist>>(playlists);
        }

        public async Task RemovePlaylistAsync(Playlist playlist)
        {
            var playlistDto = _mapper.Map<PlaylistDto>(playlist);

            await _serialQueue.Enqueue(async () => await _playlistRepository.RemoveAsync(playlistDto));
        }

        public async Task RemovePlaylistPhysicallyAsync(string playlistName)
        {
            await _serialQueue.Enqueue(async () => await _playlistRepository.RemovePhysicallyAsync(playlistName));
        }

        public async Task RemovePlaylistSeriesPhysicallyAsync(string seriesName)
        {
            await _serialQueue.Enqueue(async () => await _playlistRepository.RemoveSeriesPhysicallyAsync(seriesName));
        }

        public async Task InsertPlaylistAsync(Playlist playlist)
        {
            var playlistDto = _mapper.Map<PlaylistDto>(playlist);

            var audios = playlist.Audios;

            var playlistAudioDtos = new List<PlaylistAudioDto>();

            for (int i = 0; i < audios.Count; i++)
            {
                playlistAudioDtos.Add(new PlaylistAudioDto()
                {
                    AudioId = audios.ElementAt(i).Id,
                    PlaylistId = playlist.Id,
                    Placement = i,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                });
            }

            await _playlistRepository.InsertAsync(playlistDto);

            await _playlistAudioRepository.InsertPlaylistAudioRangeAsync(playlistAudioDtos);
        }

        public async Task<List<Playlist>> GetPlaylistSeriesAsync(string seriesName)
        {
            var series = await _playlistRepository.GetSeriesAsync(seriesName);

            return _mapper.Map<List<Playlist>>(series);
        }

        public async Task<Playlist> GetPlaylistAsync(string playlistName)
        {
            var playlist = await _playlistRepository.GetPlaylistAsync(playlistName);

            return _mapper.Map<Playlist>(playlist);
        }

        public async Task<List<Playlist>> GetPlaylistsByMonitoringItemAsync(MonitoringItem monitoringItem)
        {
            var playlists = await _playlistRepository.GetByTargetPlaylistNameAsync(monitoringItem.TargetPlaylistName);

            return _mapper.Map<List<Playlist>>(playlists);
        }

        public async Task<MonitoringItem> GetMonitoringItemByIdAsync(Guid monitoringItemId)
        {
            var monitoringItem = await _monitoringItemRepository.GetByIdAsync(monitoringItemId);

            return _mapper.Map<MonitoringItem>(monitoringItem);
        }

        public async Task ChangePlaylistVisibility(Playlist playlist, bool isPublic)
        {
            var playlistDto = _mapper.Map<PlaylistDto>(playlist);

            await _serialQueue.Enqueue(async () => await _playlistRepository.ChangePlaylistVisibility(playlistDto, isPublic));
        }

        public async Task RemoveSongFromPlaylist(Guid playlistId, string songId)
        {
            await _serialQueue.Enqueue(async () => await _playlistAudioRepository.RemoveSong(playlistId, songId));
        }

        public async Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition)
        {
            return await _serialQueue.Enqueue(async () => await _playlistAudioRepository.ChangeTrackPosition(playlistId, songId, oldPosition, newPosition));
        }
    }
}
