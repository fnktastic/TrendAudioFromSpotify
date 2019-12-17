using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IPlaylistAudioRepository
    {
        Task InsertPlaylistAudioRangeAsync(IEnumerable<PlaylistAudioDto> playlistAudioDtos);
    }

    public class PlaylistAudioRepository : IPlaylistAudioRepository
    {
        private readonly Context _context;

        public PlaylistAudioRepository(Context context)
        {
            _context = context;
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
    }
}
