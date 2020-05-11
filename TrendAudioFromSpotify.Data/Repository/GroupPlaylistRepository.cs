using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IGroupPlaylistRepository
    {
        Task InsertRangeAsync(IList<GroupPlaylistDto> groupPlaylistDtos);
        Task RemovePlaylistFromGroupAsync(Guid groupId, Guid playlistId);
    }

    public class GroupPlaylistRepository : IGroupPlaylistRepository
    {
        private readonly Context _context;

        public GroupPlaylistRepository(Context context)
        {
            _context = context;
        }

        public async Task InsertRangeAsync(IList<GroupPlaylistDto> groupPlaylistDtos)
        {
            foreach (var groupPlaylistDto in groupPlaylistDtos)
            {
                var dbEntry = await _context.GroupPlaylists.FindAsync(groupPlaylistDto.GroupId, groupPlaylistDto.PlaylistId);

                if (dbEntry == null)
                {
                    groupPlaylistDto.UpdatedAt = DateTime.UtcNow;
                    groupPlaylistDto.CreatedAt = DateTime.UtcNow;
                    _context.GroupPlaylists.Add(groupPlaylistDto);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemovePlaylistFromGroupAsync(Guid groupId, Guid playlistId)
        {
            var dbEntry = await _context.GroupPlaylists.FindAsync(groupId, playlistId);

            if (dbEntry != null)
            {
                dbEntry.UpdatedAt = DateTime.UtcNow;
                dbEntry.IsDeleted = true;

                await _context.SaveChangesAsync();
            }
        }
    }
}
