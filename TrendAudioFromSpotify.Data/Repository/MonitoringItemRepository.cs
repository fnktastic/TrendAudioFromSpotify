﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IMonitoringItemRepository
    {
        Task<List<MonitoringItemDto>> GetAllAsync();

        Task InsertAsync(MonitoringItemDto monitoringItem);
    }

    public class MonitoringItemRepository : IMonitoringItemRepository
    {
        private readonly Context _context;

        public MonitoringItemRepository(Context context)
        {
            _context = context;
        }

        public async Task<List<MonitoringItemDto>> GetAllAsync()
        {
            return await _context.MonitoringItems
                .Include(x => x.Group.GroupPlaylists.Select(y => y.Playlist))
                .Include(y => y.Trends)
                .ToListAsync();
        }

        public async Task InsertAsync(MonitoringItemDto monitoringItem)
        {
            if (monitoringItem.Id == Guid.Empty) throw new ArgumentException("Cant insert group with empty id.");

            var dbEntry = await _context.MonitoringItems.FindAsync(monitoringItem.Id);

            if (dbEntry == null)
            {
                monitoringItem.CreatedAt = DateTime.UtcNow;
                monitoringItem.UpdatedAt = DateTime.UtcNow;
                monitoringItem.Group = null;
                _context.MonitoringItems.Add(monitoringItem);
            }
            else
            {
                dbEntry.Comparison = monitoringItem.Comparison;
                dbEntry.UpdatedAt = monitoringItem.UpdatedAt;
                dbEntry.HitTreshold = monitoringItem.HitTreshold;
                dbEntry.RefreshPeriod = monitoringItem.RefreshPeriod;
                dbEntry.Top = monitoringItem.Top;
                dbEntry.TargetPlaylistName = monitoringItem.TargetPlaylistName;
                dbEntry.AutoRecreatePlaylisOnSpotify = monitoringItem.AutoRecreatePlaylisOnSpotify;
                dbEntry.PlaylistType = monitoringItem.PlaylistType;

                dbEntry.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}