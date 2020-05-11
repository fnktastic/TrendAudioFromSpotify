using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IGroupRepository
    {
        Task<List<GroupDto>> GetAllAsync(bool getDeleted = false);

        Task InsertAsync(GroupDto group);

        Task RemoveAsync(GroupDto group);
    }

    public class GroupRepository : IGroupRepository
    {
        private readonly Context _context;

        public GroupRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<GroupDto>> GetAllAsync(bool getDeleted = false)
        {
            var list = await _context.Groups
                .Where(x => x.IsDeleted == getDeleted)
                .Include(x => x.GroupPlaylists.Select(z => z.Playlist))
                .Include(x => x.MonitoringItems.Select(y => y.Trends))
                .ToListAsync();

            list.ForEach(item =>
            {
                item.GroupPlaylists = item.GroupPlaylists.Where(x => x.IsDeleted == false).ToList();
            });

            return list;
        }

        public async Task InsertAsync(GroupDto group)
        {
            if (group.Id == Guid.Empty) throw new ArgumentException("Cant insert group with empty id.");

            var dbEntry = await _context.Groups.FindAsync(group.Id);

            if (dbEntry == null)
            {
                group.CreatedAt = DateTime.UtcNow;
                group.UpdatedAt = DateTime.UtcNow;
                _context.Groups.Add(group);
            }
            else
            {
                dbEntry.Name = group.Name;
                dbEntry.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(GroupDto group)
        {
            if (group.Id == Guid.Empty) throw new ArgumentException("Cant insert group with empty id.");

            var dbEntry = await _context.Groups.FindAsync(group.Id);

            if (dbEntry != null)
            {
                dbEntry.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
