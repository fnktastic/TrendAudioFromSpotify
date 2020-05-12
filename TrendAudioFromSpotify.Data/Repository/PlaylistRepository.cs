using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        Task AddSpotifyUriHrefAsync(Guid id, string playlistId, string playlistHref, string uri, string owner, string ownerProfileUrl, string cover);

        Task RemoveAsync(PlaylistDto playlist);

        Task RemovePhysicallyAsync(string playlistName);

        Task RemoveSeriesPhysicallyAsync(string playlistName);

        Task<List<PlaylistDto>> GetSeriesAsync(string seriesName);

        Task<PlaylistDto> GetPlaylistAsync(string playlistName);

        Task<List<PlaylistDto>> GetByTargetPlaylistNameAsync(string targetPlaylistName);

        Task ChangePlaylistVisibility(PlaylistDto playlist, bool isPublic);
        Task UpdatePlaylist(PlaylistDto playlistDto);
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
                .Where(x => x.IsDeleted == false)
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
                dbEntry.Total = playlist.Total;
                dbEntry.IsDeleted = false;
                dbEntry.UpdatedAt = DateTime.UtcNow;
                _context.Entry<PlaylistDto>(dbEntry).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task InsertRangeAsync(List<PlaylistDto> playlists)
        {
            foreach (var playlist in playlists)
                await InsertAsync(playlist);
        }

        public async Task AddSpotifyUriHrefAsync(Guid id, string playlistId, string playlistHref, string uri, string owner, string ownerProfileUrl, string cover)
        {
            var playlist = _context.Playlists.Find(id);

            if (playlist != null)
            {
                playlist.SpotifyId = playlistId;
                playlist.Href = playlistHref;
                playlist.Uri = uri;
                playlist.Owner = owner;
                playlist.OwnerProfileUrl = ownerProfileUrl;
                playlist.Cover = cover;
                playlist.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(PlaylistDto playlist)
        {
            if (playlist.Id == Guid.Empty) throw new ArgumentException("Cant delete playlist with empty id.");

            var dbEntry = await _context.Playlists.FindAsync(playlist.Id);

            if (dbEntry != null)
            {
                dbEntry.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemovePhysicallyAsync(string playlistName)
        {
            if (string.IsNullOrEmpty(playlistName)) throw new ArgumentException("Cant delete playlist with empty name.");

            var dbEntry = await _context.Playlists.FirstOrDefaultAsync(x => x.Name == playlistName && x.IsDeleted == false);

            if (dbEntry != null)
            {
                var playlistItems = _context.PlaylistAudios.Where(x => x.PlaylistId == dbEntry.Id);

                foreach (var playlistItem in playlistItems)
                    _context.Entry<PlaylistAudioDto>(playlistItem).State = EntityState.Deleted;

                _context.Entry<PlaylistDto>(dbEntry).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveSeriesPhysicallyAsync(string playlistName)
        {
            if (string.IsNullOrEmpty(playlistName)) throw new ArgumentException("Cant delete playlist with empty name.");

            var dbEntry = await _context.Playlists.FirstOrDefaultAsync(x => x.Name == playlistName && x.IsDeleted == false);

            if (dbEntry != null)
            {
                var seriesKey = dbEntry.SeriesKey;

                var series = _context.Playlists.Where(x => x.SeriesKey == seriesKey && x.IsDeleted == false);

                foreach (var volume in series)
                {
                    var volumeItems = _context.PlaylistAudios.Where(x => x.PlaylistId == volume.Id);

                    foreach (var volumeItem in volumeItems)
                        _context.Entry<PlaylistAudioDto>(volumeItem).State = EntityState.Deleted;

                    _context.Entry<PlaylistDto>(volume).State = EntityState.Deleted;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<PlaylistDto>> GetSeriesAsync(string seriesName)
        {
            var series = await _context.Playlists
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Name == seriesName && x.IsSeries == true)
                .Include(x => x.PlaylistAudios.Select(y => y.Audio))
                .ToListAsync();

            return series;
        }

        public async Task<PlaylistDto> GetPlaylistAsync(string playlistName)
        {
            var playlist = await _context.Playlists
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Name == playlistName && x.IsSeries == false)
                .Include(x => x.PlaylistAudios.Select(y => y.Audio))
                .FirstOrDefaultAsync();

            return playlist;
        }

        public async Task<List<PlaylistDto>> GetByTargetPlaylistNameAsync(string targetPlaylistName)
        {
            var playlists = await _context.Playlists
                .Where(x => x.IsDeleted == false)
                .Where(x => x.MadeByUser == true && x.Name == targetPlaylistName)
                .Include(x => x.PlaylistAudios.Select(y => y.Audio))
                .ToListAsync();

            return playlists;
        }

        public async Task ChangePlaylistVisibility(PlaylistDto playlistDto, bool isPublic)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(x => x.Id == playlistDto.Id);

            if (playlist != null)
            {
                playlist.IsPublic = isPublic;

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePlaylist(PlaylistDto playlistDto)
        {
            var dbEntry = await _context.Playlists.FirstOrDefaultAsync(x => x.Id == playlistDto.Id);

            if(dbEntry != null)
            {
                _context.Entry(dbEntry).CurrentValues.SetValues(playlistDto);
            }

            await _context.SaveChangesAsync();
        }
    }
}
