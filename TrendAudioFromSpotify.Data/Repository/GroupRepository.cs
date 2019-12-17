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
    public interface IGroupRepository
    {
        Task<List<GroupDto>> GetAllAsync();

        Task InsertAsync(GroupDto group);
    }

    public class GroupRepository : IGroupRepository
    {
        private readonly Context _context;

        public GroupRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<GroupDto>> GetAllAsync()
        {
            return await _context.Groups
                .Include(x => x.GroupPlaylists.Select(y => y.Playlist))
                .Include(x => x.MonitoringItems.Select(y => y.Trends))
                .ToListAsync();
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
    }
}
