using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IMonitoringItemAudioRepository
    {
        Task InsertRangeAsync(IList<MonitoringItemAudioDto> monitoringItemAudioDtos);
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
                var dbEntry = await _context.GroupPlaylists.FindAsync(monitoringItemAudioDto.MonitoringItemId, monitoringItemAudioDto.AudioId);

                if (dbEntry == null)
                {
                    monitoringItemAudioDto.UpdatedAt = DateTime.UtcNow;
                    monitoringItemAudioDto.CreatedAt = DateTime.UtcNow;
                    _context.MonitoringItemAudios.Add(monitoringItemAudioDto);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
