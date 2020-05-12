using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IMonitoringItemAudioRepository
    {
        Task InsertRangeAsync(IList<MonitoringItemAudioDto> monitoringItemAudioDtos);

        Task<List<MonitoringItemAudioDto>> GetAllByMonitoringItemIdAsync(Guid monitoringItemId);
        Task RemoveRangeAsync(Guid monitoringItemId);
        Task<List<MonitoringItemAudioDto>> GetTrends(Guid monitoringItemId);
    }

    public class MonitoringItemAudioRepository : IMonitoringItemAudioRepository
    {
        private readonly Context _context;

        public MonitoringItemAudioRepository(Context context)
        {
            _context = context;
        }

        public async Task InsertRangeAsync(IList<MonitoringItemAudioDto> monitoringItemAudioDtos)
        {
            foreach (var monitoringItemAudioDto in monitoringItemAudioDtos)
            {
                var dbEntry = await _context.MonitoringItemAudios.FindAsync(monitoringItemAudioDto.MonitoringItemId, monitoringItemAudioDto.AudioId);

                if (dbEntry == null)
                {
                    monitoringItemAudioDto.UpdatedAt = DateTime.UtcNow;
                    monitoringItemAudioDto.CreatedAt = DateTime.UtcNow;
                    _context.MonitoringItemAudios.Add(monitoringItemAudioDto);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<MonitoringItemAudioDto>> GetAllByMonitoringItemIdAsync(Guid monitoringItemId)
        {
            return await _context.MonitoringItemAudios
                .Where(x => x.MonitoringItemId == monitoringItemId)
                .Include(x => x.Audio.PlaylistAudios.Select(y => y.Playlist))
                .ToListAsync();
        }

        public async Task RemoveRangeAsync(Guid monitoringItemId)
        {
            var monitoringItemAudios = await _context.MonitoringItemAudios.Where(x => x.MonitoringItemId == monitoringItemId).ToListAsync();

            if (monitoringItemAudios != null && monitoringItemAudios.Count > 0)
            {
                monitoringItemAudios.ForEach(x =>
                {
                    _context.Entry<MonitoringItemAudioDto>(x).State = EntityState.Deleted;
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<MonitoringItemAudioDto>> GetTrends(Guid monitoringItemId)
        {
            return await _context.MonitoringItemAudios
                .Where(x => x.MonitoringItemId == monitoringItemId)
                .Include(x => x.Audio)
                .ToListAsync();
        }
    }
}
