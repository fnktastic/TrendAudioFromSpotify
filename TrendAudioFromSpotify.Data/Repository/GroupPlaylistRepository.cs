using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IGroupPlaylistRepository
    {
        Task InsertRangeAsync(IList<GroupPlaylistDto> groupPlaylistDtos);
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

                if(dbEntry == null)
                {
                    groupPlaylistDto.UpdatedAt = DateTime.UtcNow;
                    groupPlaylistDto.CreatedAt = DateTime.UtcNow;
                    _context.GroupPlaylists.Add(groupPlaylistDto);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
