using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IGroupPlaylistRepository
    {
        Task InsertRangeAsync(IList<GroupPlaylistDto> groupPlaylistDtos);
        Task ChangePosition(Guid groupId, Guid playlistId, int oldPosition, int newPosition);
        Task RemovePlaylistFromGroupAsync(Guid groupId, Guid playlistId);
        Task RemovePhysically(Guid groupId);
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

        public async Task ChangePosition(Guid groupId, Guid playlistId, int oldPosition, int newPosition)
        {
            var groupPlaylists = await _context.GroupPlaylists.Where(x => x.GroupId == groupId && x.IsDeleted == false).OrderBy(x => x.Placement).ToListAsync();

            var targetGroupPlaylist = groupPlaylists.FirstOrDefault(x => x.PlaylistId == playlistId);

            if (targetGroupPlaylist == null) return;

            groupPlaylists.Remove(targetGroupPlaylist);

            groupPlaylists.Insert(newPosition, targetGroupPlaylist);

            for (int i = 0; i < groupPlaylists.Count; i++)
            {
                groupPlaylists.ElementAt(i).Placement = i + 1;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemovePhysically(Guid groupId)
        {
            var groupPlaylists = await _context.GroupPlaylists.Where(x => x.GroupId == groupId).ToListAsync();

            foreach(var groupPlaylist in groupPlaylists)
            {
                _context.Entry<GroupPlaylistDto>(groupPlaylist).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }
    }
}
