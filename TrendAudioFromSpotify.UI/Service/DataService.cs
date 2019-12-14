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
        Task InsertGroupAsync(Group group);
        Task InsertPlaylistRangeAsync(IEnumerable<Playlist> playlists);
        Task InsertGroupPlaylistRangeAsync(Group group);
        Task<List<Group>> GetAllGroups();
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

        public DataService(
            SerialQueue serialQueue,
            IMapper mapper,
            IAudioRepository audioRepository,
            IGroupPlaylistRepository groupPlaylistRepository,
            IGroupRepository groupRepository,
            IPlaylistAudioRepository playlistAudioRepository,
            IPlaylistRepository playlistRepository
            )
        {
            _serialQueue = serialQueue;
            _mapper = mapper;
            _audioRepository = audioRepository;
            _groupPlaylistRepository = groupPlaylistRepository;
            _groupRepository = groupRepository;
            _playlistAudioRepository = playlistAudioRepository;
            _playlistRepository = playlistRepository;
        }

        public async Task InsertAudioAsync(Audio audio)
        {
            await _audioRepository.InsertAsync(_mapper.Map<AudioDto>(audio));
        }

        public async Task InsertGroupAsync(Group group)
        {
            await _groupRepository.InsertAsync(_mapper.Map<GroupDto>(group));
        }

        public async Task InsertPlaylistRangeAsync(IEnumerable<Playlist> playlists)
        {
            await _playlistRepository.InsertRangeAsync(_mapper.Map<List<PlaylistDto>>(playlists));
        }

        public async Task InsertGroupPlaylistRangeAsync(Group group)
        {
            await _groupPlaylistRepository.InsertRangeAsync(group.Playlists.Select(x => new GroupPlaylistDto()
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                GroupId = group.Id,
                PlaylistId = x.Id
            }).ToList());
        }

        public async Task<List<Group>> GetAllGroups()
        {
            var groups = await _groupRepository.GetAllAsync();

            return _mapper.Map<List<Group>>(groups);
        }
    }
}
