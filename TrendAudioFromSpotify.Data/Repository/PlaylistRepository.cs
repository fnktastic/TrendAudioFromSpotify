using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IPlaylistRepository
    {
        Task<List<PlaylistDto>> GetAllAsync();

        Task InsertAsync(PlaylistDto playlist);
    }

    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly Context _context;

        public PlaylistRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<PlaylistDto>> GetAllAsync()
        {
            return await _context.Playlists.ToListAsync();
        }

        public async Task InsertAsync(PlaylistDto playlist)
        {
            if (string.IsNullOrEmpty(playlist.Id) == false)
            {
                var dbEntry = await _context.Playlists.FindAsync(playlist.Id);

                if (dbEntry == null)
                {
                    playlist.CreatedAt = DateTime.UtcNow;
                    playlist.UpdatedAt = DateTime.UtcNow;
                    _context.Playlists.Add(playlist);
                }
                else
                {
                    dbEntry.Href = playlist.Href;
                    dbEntry.Name = playlist.Name;
                    dbEntry.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
