using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.DataAccess
{
    public class Context : DbContext
    {
        public Context() : base("TrendifyDb")
        {

        }

        public DbSet<AudioDto> Audios { get; set; }

        public DbSet<PlaylistDto> Playlists { get; set; }

        public DbSet<PlaylistAudioDto> PlaylistAudios { get; set; }

        public DbSet<GroupDto> Groups { get; set; }

        public DbSet<GroupPlaylistDto> GroupPlaylists { get; set; }

        public DbSet<MonitoringItemDto> MonitoringItems { get; set; }

        public DbSet<MonitoringItemAudioDto> MonitoringItemAudios { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AudioDto>().HasKey(a => a.Id);

            modelBuilder.Entity<PlaylistDto>().HasKey(p => p.Id);

            modelBuilder.Entity<GroupDto>().HasKey(g => g.Id);

            modelBuilder.Entity<MonitoringItemDto>().HasKey(m => m.Id);

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasKey(pa => new { pa.PlaylistId, pa.AudioId });

            modelBuilder.Entity<GroupPlaylistDto>()
                .HasKey(ga => new { ga.GroupId, ga.PlaylistId });

            modelBuilder.Entity<MonitoringItemAudioDto>()
                .HasKey(mi => new { mi.MonitoringItemId, mi.AudioId });

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasRequired(pa => pa.Audio)
                .WithMany(p => p.PlaylistAudios)
                .HasForeignKey(pc => pc.AudioId);

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasRequired(pc => pc.Playlist)
                .WithMany(c => c.PlaylistAudios)
                .HasForeignKey(pc => pc.PlaylistId);

            modelBuilder.Entity<GroupPlaylistDto>()
                .HasRequired(gp => gp.Group)
                .WithMany(p => p.GroupPlaylists)
                .HasForeignKey(pc => pc.GroupId);

            modelBuilder.Entity<GroupPlaylistDto>()
                .HasRequired(gp => gp.Playlist)
                .WithMany(g => g.GroupPlaylists)
                .HasForeignKey(gc => gc.PlaylistId);

            modelBuilder.Entity<MonitoringItemAudioDto>()
                .HasRequired(mi => mi.MonitoringItem)
                .WithMany(m => m.Trends)
                .HasForeignKey(mt => mt.MonitoringItemId);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class DataInitializer : DropCreateDatabaseIfModelChanges<Context>
    {
        public DataInitializer()
        {
            using (var context = new Context())
            {
                InitializeDatabase(context);
            }
        }
    }
}
