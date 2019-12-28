using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task AddSpotifyUriHrefToMonitoringItemAsync(Guid id, string playlistId, string playlistHref);
        Task<List<Playlist>> GetAllPlaylistsAsync();
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

            var audios = _mapper.Map<List<Audio>>(monitoringItemAudios.Select(x => x.Audio ));

            audios.ForEach(x => x.Hits = monitoringItemAudios.First(y => y.AudioId == x.Id).Hits);

            return audios.OrderByDescending(x => x.Hits).ToList();
        }

        public async Task InsertPlaylistAudioRangeAsync(IEnumerable<Audio> audios)
        {
            var playlistAudioDtos = new List<PlaylistAudioDto>();

            foreach(var audio in audios)
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

        public async Task AddSpotifyUriHrefToMonitoringItemAsync(Guid id, string playlistId, string playlistHref)
        {
            await _serialQueue.Enqueue(async () => await _monitoringItemRepository.AddSpotifyUriHrefAsync(id, playlistId, playlistHref));
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            var playlists = await _serialQueue.Enqueue(async () => await _playlistRepository.GetAllAsync());

            return _mapper.Map<List<Playlist>>(playlists);
        }
    }
}
