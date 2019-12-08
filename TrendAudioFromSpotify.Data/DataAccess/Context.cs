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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AudioDto>().HasKey(a => a.Id);

            modelBuilder.Entity<PlaylistDto>().HasKey(p => p.Id);

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasKey(pa => new { pa.PlaylistId, pa.AudioId });

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasRequired(pa => pa.Audio)
                .WithMany(p => p.PlaylistAudios)
                .HasForeignKey(pc => pc.AudioId);

            modelBuilder.Entity<PlaylistAudioDto>()
                .HasRequired(pc => pc.Playlist)
                .WithMany(c => c.PlaylistAudios)
                .HasForeignKey(pc => pc.PlaylistId);

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
