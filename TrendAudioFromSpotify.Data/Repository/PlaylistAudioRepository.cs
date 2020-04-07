using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IPlaylistAudioRepository
    {
        Task InsertPlaylistAudioRangeAsync(IEnumerable<PlaylistAudioDto> playlistAudioDtos);
        Task RemoveSong(Guid playlistId, string songId);
        Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition);
        Task SendToPlaylist(string audioId, Guid playlistId, int newPosition);
    }

    public class PlaylistAudioRepository : IPlaylistAudioRepository
    {
        private readonly Context _context;

        public PlaylistAudioRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<string>> ChangeTrackPosition(Guid playlistId, string songId, int oldPosition, int newPosition)
        {
            var audios = await _context.PlaylistAudios.Where(x => x.PlaylistId == playlistId).OrderBy(x => x.Placement).ToListAsync();

            var targetAudio = audios.FirstOrDefault(x => x.AudioId == songId);

            audios.Remove(targetAudio);

            audios.Insert(newPosition, targetAudio);

            for (int i = 0; i < audios.Count; i++)
            {
                audios.ElementAt(i).Placement = i + 1;
            }

            await _context.SaveChangesAsync();

            return audios.Select(x => x.Audio.Uri).ToList();
        }

        public async Task InsertPlaylistAudioRangeAsync(IEnumerable<PlaylistAudioDto> playlistAudioDtos)
        {
            foreach (var playlistAudioDto in playlistAudioDtos)
            {
                var dbEntry = await _context.PlaylistAudios.FindAsync(playlistAudioDto.PlaylistId, playlistAudioDto.AudioId);

                if (dbEntry == null)
                {
                    playlistAudioDto.UpdatedAt = DateTime.UtcNow;
                    playlistAudioDto.CreatedAt = DateTime.UtcNow;
                    _context.PlaylistAudios.Add(playlistAudioDto);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveSong(Guid playlistId, string songId)
        {
            var dbEntry = await _context.PlaylistAudios.FirstOrDefaultAsync(x => x.PlaylistId == playlistId && x.AudioId == songId);

            if (dbEntry != null)
            {
                _context.Entry<PlaylistAudioDto>(dbEntry).State = EntityState.Deleted;

                await _context.SaveChangesAsync();
            }
        }

        public async Task SendToPlaylist(string audioId, Guid playlistId, int newPosition)
        {
            var audios = await _context.PlaylistAudios.Where(x => x.PlaylistId == playlistId).OrderBy(x => x.Placement).ToListAsync();

            var targetAudio = _context.PlaylistAudios.Add(new PlaylistAudioDto()
            {
                AudioId = audioId,
                PlaylistId = playlistId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Placement = newPosition
            });

            audios.Insert(newPosition, targetAudio);

            for (int i = 0; i < audios.Count; i++)
            {
                audios.ElementAt(i).Placement = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}
