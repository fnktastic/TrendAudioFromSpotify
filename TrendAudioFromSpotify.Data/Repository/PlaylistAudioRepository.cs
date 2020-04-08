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
        Task<int> RecalcTotal(Guid playlistId);
        Task ClearPlaylist(Guid playlistId);
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
            try
            {
                var audios = await _context.PlaylistAudios.Where(x => x.PlaylistId == playlistId).OrderBy(x => x.Placement).ToListAsync();

                var targetAudio = audios.FirstOrDefault(x => x.AudioId == songId);

                audios.Remove(targetAudio);

                if (targetAudio == null) return null;

                audios.Insert(newPosition, targetAudio);

                for (int i = 0; i < audios.Count; i++)
                {
                    audios.ElementAt(i).Placement = i + 1;
                }

                await _context.SaveChangesAsync();

                return audios.Select(x => x.Audio.Uri).ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task ClearPlaylist(Guid playlistId)
        {
            var audios = await _context.PlaylistAudios.Where(x => x.PlaylistId == playlistId).ToListAsync();

            foreach(var audio in audios)
            {
                _context.Entry<PlaylistAudioDto>(audio).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
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

        public async Task<int> RecalcTotal(Guid playlistId)
        {
            var total = await _context.PlaylistAudios.Where(x => x.PlaylistId == playlistId).CountAsync();

            var playlist = await _context.Playlists.FirstOrDefaultAsync(x => x.Id == playlistId);

            if(playlist != null)
            {
                playlist.Total = total;

                playlist.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return total;
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
