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
        Task<List<PlaylistDto>> GetAllAsync(bool madeByUser = true);

        Task InsertAsync(PlaylistDto playlist);

        Task InsertRangeAsync(List<PlaylistDto> playlists);

        Task AddSpotifyUriHrefAsync(Guid id, string playlistId, string playlistHref);

        Task RemoveAsync(PlaylistDto playlist);
    }

    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly Context _context;

        public PlaylistRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<PlaylistDto>> GetAllAsync(bool madeByUser = true)
        {
            return await _context.Playlists
                .Where(x => x.MadeByUser == madeByUser)
                .Include(x => x.PlaylistAudios.Select(y => y.Audio))
                .ToListAsync();
        }

        public async Task InsertAsync(PlaylistDto playlist)
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

        public async Task InsertRangeAsync(List<PlaylistDto> playlists)
        {
            foreach (var playlist in playlists)
                await InsertAsync(playlist);
        }

        public async Task AddSpotifyUriHrefAsync(Guid id, string playlistId, string playlistHref)
        {
            var playlist = _context.Playlists.Find(id);

            if (playlist != null)
            {
                playlist.SpotifyId = playlistId;
                playlist.Href = playlistHref;
                playlist.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(PlaylistDto playlist)
        {
            if (playlist.Id == Guid.Empty) throw new ArgumentException("Cant insert group with empty id.");

            var dbEntry = await _context.Playlists.FindAsync(playlist.Id);

            if (dbEntry != null)
            {
                dbEntry.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
